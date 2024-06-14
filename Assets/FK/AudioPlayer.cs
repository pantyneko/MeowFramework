using System;
using UnityEngine;

namespace Panty
{
    public interface IAudioPlayer : IModule
    {
        void PlayBgm(string name, float clipVolume = 1f);
        void PlayBgmAsync(string name, float clipVolume = 1f);
        void PlaySound(string name, float clipVolume = 1f);
        void PlaySoundAsync(string name, float clipVolume = 1f);
        void PlaySoundCall(string name, float clipVolume = 1f);
        void StopBgm();
        void PauseBgm();

        Sound GetSound(string name, float clipVolume = 1f);
        ValueBinder<float> BgmVolume { get; }
        ValueBinder<float> SoundVolume { get; }
    }
    public class Sound
    {
        private float clipVolume;
        private AudioSource source;
        public Predicate<AudioSource> onUpdate;
        public Sound(AudioSource source)
        {
            this.source = source;
        }
        public Sound Set(float volume)
        {
            clipVolume = volume;
            return this;
        }
        public void Play() => source.Play();
        public void Pause() => source.Pause();
        public void Stop() => source.Stop();
        public void SetClip(AudioClip clip, bool loop)
        {
            source.clip = clip;
            source.loop = loop;
        }
        public void Update()
        {
            if (onUpdate == null) return;
            if (onUpdate.Invoke(source))
            {
                onUpdate = null;
            }
        }
        public void Volume(float newValue)
        {
            source.volume = clipVolume * newValue;
        }
        public bool IsPlaying => source.loop || source.isPlaying;
    }
    public class AudioPlayer : AbsModule, IAudioPlayer
    {
        public static Predicate<AudioSource> Call;

        private static float bgmClipVolume;

        private IResLoader mLoader;

        private Fade fade;
        private AudioSource mBGM;
        private GameObject mRoot;
        private PArray<Sound> mOpenList, mCloseList;

        public ValueBinder<float> BgmVolume { get; } = 0.5f;
        public ValueBinder<float> SoundVolume { get; } = 0.5f;

        protected override void OnInit()
        {
            mOpenList = new PArray<Sound>(8);
            mCloseList = new PArray<Sound>(8);

            mLoader = this.Module<IResLoader>();

            fade = new Fade(0f, BgmVolume);
            BgmVolume.RegisterWithInitValue(OnBgmVolumeChanged);
            SoundVolume.RegisterWithInitValue(OnSoundVolumeChanged);
        }
        private void InitBgm()
        {
            if (mRoot == null) InitRoot();
            mBGM = mRoot.AddComponent<AudioSource>();
            mBGM.loop = true;
            mBGM.volume = 0;
            MonoKit.OnUpdate += OnUpdate;
        }
        void IAudioPlayer.PlayBgm(string name, float clipVolume)
        {
            if (mBGM == null) InitBgm();
            if (mBGM.clip == null || mBGM.clip.name == name) return;
            bgmClipVolume = clipVolume;
            mLoader.AsyncLoad<AudioClip>(name, TryPlay);
        }
        async void IAudioPlayer.PlayBgmAsync(string name, float clipVolume)
        {
            if (mBGM == null) InitBgm();
            if (mBGM.clip == null || mBGM.clip.name == name) return;
            bgmClipVolume = clipVolume;
            TryPlay(await mLoader.AsyncLoad<AudioClip>(name));
        }
        void IAudioPlayer.StopBgm()
        {
            if (mBGM == null) return;
            if (mBGM.isPlaying)
            {
                fade.Out();
                fade.Set(mBGM.Stop);
            }
        }
        void IAudioPlayer.PauseBgm()
        {
            if (mBGM == null) return;
            if (mBGM.isPlaying)
            {
                fade.Out();
                fade.Set(mBGM.Pause);
            }
        }
        void IAudioPlayer.PlaySound(string name, float clipVolume)
        {
            GetSound(mLoader.SyncLoadFromCache<AudioClip>(name), clipVolume, false).Play();
        }
        async void IAudioPlayer.PlaySoundAsync(string name, float clipVolume)
        {
            GetSound(await mLoader.AsyncLoadFromCache<AudioClip>(name), clipVolume, false).Play();
        }
        void IAudioPlayer.PlaySoundCall(string name, float clipVolume)
        {
            mLoader.AsyncLoadFromCache<AudioClip>(name, clip => GetSound(clip, clipVolume, false).Play());
        }
        private Sound GetSound(AudioClip clip, float clipVolume, bool loop = false)
        {
            TryGetSource(clipVolume, out var sound);
            sound.SetClip(clip, loop);
            sound.Volume(SoundVolume);
            if (Call != null)
            {
                sound.onUpdate = Call;
                Call = null;
            }
            return sound;
        }
        Sound IAudioPlayer.GetSound(string name, float clipVolume)
        {
            return GetSound(mLoader.SyncLoadFromCache<AudioClip>(name), clipVolume, true);
        }
        private void TryGetSource(float clipVolume, out Sound sound)
        {
            if (mCloseList.IsEmpty)
            {
                if (mRoot == null) InitRoot();
                int i = 0;
                while (i < mOpenList.Count)
                {
                    sound = mOpenList[i];
                    if (sound.IsPlaying) i++;
                    else
                    {
                        mOpenList.RmvAt(i);
                        mCloseList.Push(sound);
                    }
                }
                sound = mCloseList.IsEmpty ?
                    new Sound(mRoot.AddComponent<AudioSource>()) : mCloseList.Pop();
                mOpenList.Push(sound.Set(clipVolume));
            }
            else
            {
                sound = mCloseList.Pop();
                mOpenList.Push(sound.Set(clipVolume));
            }
        }
        private void OnBgmVolumeChanged(float v)
        {
            if (mBGM == null) return;
            mBGM.volume = v * bgmClipVolume;
        }
        private void OnSoundVolumeChanged(float v)
        {
            foreach (var sound in mOpenList)
                sound.Volume(v);
        }
        private void OnUpdate()
        {
            for (int i = 0; i < mOpenList.Count; i++)
                mOpenList[i].Update();
            if (fade.IsClose) return;
            fade.Update(Time.deltaTime);
            mBGM.volume = fade.Cur;
        }
        private void TryPlay(AudioClip clip)
        {
            if (mBGM.isPlaying)
            {
                fade.Out();
                fade.Set(() => Play(clip));
            }
            else Play(clip);
        }
        private void Play(AudioClip clip)
        {
            fade.In();
            fade.Set(null);
            fade.Max = BgmVolume * bgmClipVolume;
            mBGM.clip = clip;
            mBGM.Play();
        }
        // 初始化根节点
        private void InitRoot()
        {
            mRoot = new GameObject("AudioPool");
            GameObject.DontDestroyOnLoad(mRoot);
        }
    }
}