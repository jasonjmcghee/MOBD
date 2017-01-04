using UnityEngine;
using System.Collections;

namespace Completed
{
	public class Enemy : MovingEntity {

        // TODO: Change this to an ability (or multiple)
        public int damage = 0;
        public int rateOfFire = 0;

        public int defense = 0;
        public int health = 1;

        private Vector3 lastPlayerPos = Vector3.zero;

        protected override void Start () {

            // GameManager.instance.AddEnemyToList (this);
            AnimatedSprite enemySprite = new AnimatedSprite();
            enemySprite.SetRenderers(GetComponentsInChildren<SpriteRenderer>());
            gameObject.AddComponent<Animation>();
            animation = GetComponent<Animation>();
             
            Transform[] transforms = GetComponentsInChildren<Transform>();
            AnimatedSpriteManager.SetIdleAnimation(animation, transforms);
            AnimatedSpriteManager.SetMoveAnimation(animation, transforms);
            base.Start();
        }

        public Enemy (int damage, int rateOfFire, int defense, int health, float speed) {
            this.damage = damage;
            this.rateOfFire = rateOfFire;
            this.defense = defense;
            this.health = health;
            base.speed = speed;
        }

        public Enemy () {
            base.speed = 50f;
        }

        public void FixedUpdate () {
            base.FixedUpdate();
            if (!lastPlayerPos.Equals(Player.Position)) {
                lastPlayerPos = Player.Position;
                Move(lastPlayerPos);
            }
        }
    }
}
