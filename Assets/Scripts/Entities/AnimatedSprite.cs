using System;
using UnityEngine;

namespace Completed {
	public class AnimatedSprite {

		private Sprite head;
		private Sprite body;
		private Sprite leftHand;
		private Sprite rightHand;
		private Sprite leftFoot;
		private Sprite rightFoot;

		// TODO: instead of strings we should use classes for each (Head, Body, Hand, Foot) which have a Sprite along with other data
		public AnimatedSprite (String head, String body, String leftHand, String rightHand, String leftFoot, String rightFoot) {
			this.head = AnimatedSpriteManager.getHead (head);
			this.body = AnimatedSpriteManager.getBody (body);
			this.leftHand = AnimatedSpriteManager.getHand (leftHand);
			this.rightHand = AnimatedSpriteManager.getHand (rightHand);
			this.leftFoot = AnimatedSpriteManager.getFoot (leftFoot);
			this.rightFoot = AnimatedSpriteManager.getFoot (rightFoot);
		}

		// TODO: we shouldn't have this... it should randomly generate a Head instance for example and find it's mapping
		public AnimatedSprite () {
			this.head = AnimatedSpriteManager.getRandomHead ();
			this.body = AnimatedSpriteManager.getRandomBody ();
			this.leftHand = AnimatedSpriteManager.getRandomHand ();
			this.rightHand = AnimatedSpriteManager.getRandomHand ();
			this.leftFoot = AnimatedSpriteManager.getRandomFoot ();
			this.rightFoot = AnimatedSpriteManager.getRandomFoot ();
		}

		public void SetRenderers(SpriteRenderer[] renderers) {
			foreach (SpriteRenderer renderer in renderers) {
				if (renderer.name == "head") {
					renderer.sprite = this.head;
				} else if (renderer.name == "body") {
					renderer.sprite = this.body;
				} else if (renderer.name == "leftHand") {
					renderer.sprite = this.leftHand;
				} else if (renderer.name == "rightHand") {
					renderer.sprite = this.rightHand;
				} else if (renderer.name == "leftFoot") {
					renderer.sprite = this.leftFoot;
				} else if (renderer.name == "rightFoot") {
					renderer.sprite = this.rightFoot;
				}
			}
		}
	}
}

