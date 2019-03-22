// <copyright file="SoundEffects.cs" company="Adrian Conlon">
// Copyright (c) Adrian Conlon. All rights reserved.
// </copyright>

namespace Invaders
{
    using System;
    using Microsoft.Xna.Framework.Audio;
    using Microsoft.Xna.Framework.Content;

    public class SoundEffects : IDisposable
    {
        private bool enabled;

        private SoundEffect ufoEffect;
        private SoundEffect shotEffect;
        private SoundEffect ufoDieEffect;
        private SoundEffect playerDieEffect;
        private SoundEffect invaderDieEffect;
        private SoundEffect extendEffect;
        private SoundEffect walk1Effect;
        private SoundEffect walk2Effect;
        private SoundEffect walk3Effect;
        private SoundEffect walk4Effect;

        private bool disposed = false;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void LoadContent(ContentManager content)
        {
            this.ufoEffect = LoadEffect(content, "Ufo");
            this.shotEffect = LoadEffect(content, "Shot");
            this.playerDieEffect = LoadEffect(content, "BaseHit");
            this.ufoDieEffect = LoadEffect(content, "UfoHit");
            this.invaderDieEffect = LoadEffect(content, "InvHit");
            this.extendEffect = LoadEffect(content, "Extend");
            this.walk1Effect = LoadEffect(content, "Walk1");
            this.walk2Effect = LoadEffect(content, "Walk2");
            this.walk3Effect = LoadEffect(content, "Walk3");
            this.walk4Effect = LoadEffect(content, "Walk4");
        }

        public void Enable() => this.enabled = true;

        public void Disable() => this.enabled = false;

        public void PlayUfo() => this.MaybePlayEffect(this.ufoEffect);

        public void PlayShot() => this.MaybePlayEffect(this.shotEffect);

        public void PlayUfoDie() => this.MaybePlayEffect(this.ufoDieEffect);

        public void PlayPlayerDie() => this.MaybePlayEffect(this.playerDieEffect);

        public void PlayInvaderDie() => this.MaybePlayEffect(this.invaderDieEffect);

        public void PlayExtend() => this.MaybePlayEffect(this.extendEffect);

        public void PlayWalk1() => this.MaybePlayEffect(this.walk1Effect);

        public void PlayWalk2() => this.MaybePlayEffect(this.walk2Effect);

        public void PlayWalk3() => this.MaybePlayEffect(this.walk3Effect);

        public void PlayWalk4() => this.MaybePlayEffect(this.walk4Effect);

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.ufoEffect?.Dispose();
                    this.shotEffect?.Dispose();
                    this.ufoDieEffect?.Dispose();
                    this.playerDieEffect?.Dispose();
                    this.invaderDieEffect?.Dispose();
                    this.extendEffect?.Dispose();
                    this.walk1Effect?.Dispose();
                    this.walk2Effect?.Dispose();
                    this.walk3Effect?.Dispose();
                    this.walk4Effect?.Dispose();
                }

                this.disposed = true;
            }
        }

        private static SoundEffect LoadEffect(ContentManager content, string name)
        {
            var directory = Configuration.SoundDirectory;
            var path = directory + name;
            return content.Load<SoundEffect>(path);
        }

        private void MaybePlayEffect(SoundEffect effect)
        {
            if (this.enabled)
            {
                effect.Play();
            }
        }
    }
}
