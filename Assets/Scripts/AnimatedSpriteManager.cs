using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Completed {

	public class AnimatedSpriteManager {

		private static AnimatedSpriteManager instance;
        private static String root = "Sprites/";
		private Dictionary<string, Sprite> headLookup = new Dictionary<string, Sprite>();
		private Dictionary<string, Sprite> bodyLookup = new Dictionary<string, Sprite>();
		private Dictionary<string, Sprite> handLookup = new Dictionary<string, Sprite>();
		private Dictionary<string, Sprite> footLookup = new Dictionary<string, Sprite>();

		public AnimatedSpriteManager () {
			if (AnimatedSpriteManager.instance == null) {
                AnimatedSpriteManager.instance = this;

                foreach (Sprite sprite in Resources.LoadAll<Sprite>(root + "Heads"))
                    headLookup.Add (sprite.name, sprite);
                foreach (Sprite sprite in Resources.LoadAll<Sprite>(root + "Bodies"))
                    bodyLookup.Add(sprite.name, sprite);
                foreach (Sprite sprite in Resources.LoadAll<Sprite>(root + "Hands"))
                    handLookup.Add(sprite.name, sprite);
                foreach (Sprite sprite in Resources.LoadAll<Sprite>(root + "Feet"))
                    footLookup.Add(sprite.name, sprite);
			}
		}

		public static Sprite getHead(String name) {
			Sprite head = null;
			AnimatedSpriteManager.instance.headLookup.TryGetValue (name, out head);
			return head;
		}

		public static Sprite getRandomHead() {
			Dictionary<string, Sprite>.ValueCollection values = AnimatedSpriteManager.instance.headLookup.Values;
			return values.ElementAt (Random.Range (0, values.Count));
		}

		public static Sprite getBody(String name) {
			Sprite body = null;
			AnimatedSpriteManager.instance.bodyLookup.TryGetValue (name, out body);
			return body;
		}

		public static Sprite getRandomBody() {
			Dictionary<string, Sprite>.ValueCollection values = AnimatedSpriteManager.instance.bodyLookup.Values;
			return values.ElementAt (Random.Range (0, values.Count));
		}

		public static Sprite getHand(String name) {
			Sprite hand = null;
			AnimatedSpriteManager.instance.handLookup.TryGetValue (name, out hand);
			return hand;
		}

		public static Sprite getRandomHand() {
			Dictionary<string, Sprite>.ValueCollection values = AnimatedSpriteManager.instance.handLookup.Values;
			return values.ElementAt (Random.Range (0, values.Count));
		}

		public static Sprite getFoot(String name) {
			Sprite foot = null;
			AnimatedSpriteManager.instance.footLookup.TryGetValue (name, out foot);
			return foot;
		}

		public static Sprite getRandomFoot() {
			Dictionary<string, Sprite>.ValueCollection values = AnimatedSpriteManager.instance.footLookup.Values;
			return values.ElementAt (Random.Range (0, values.Count));
		}
			
		private static AnimationCurve CreateCurve(float step, float[] values, float start = 0f) {
			Keyframe[] keys = new Keyframe[values.Length];
			for (int i = 0; i < values.Length; i++) {
				keys [i] = new Keyframe (start + (i * step), values [i]);
			}
			return new AnimationCurve (keys);
		}

        public static void SetIdleAnimation(Animation animation, Transform[] transforms) {
            AnimationClip clip = new AnimationClip();
            clip.name = "Idle";

            float start = 0f;
            foreach (Transform part in transforms) {
                if (part.name == "body") {
                    start = part.localScale.y;
                    float[] bodyScale = { start, start + .08f, start };
                    clip.SetCurve(part.name, typeof(Transform), "localScale.y", CreateCurve(0.5f, bodyScale));
                    SetRotationOnClip(clip, part, 0f, true);
                } else if (part.name == "head") {
                    start = part.localPosition.y;
                    float[] headPosition = { start, start - 0.04f, start };
                    clip.SetCurve(part.name, typeof(Transform), "localPosition.y", CreateCurve(0.5f, headPosition));
                } else if (part.name == "leftHand") {
                    start = part.localPosition.y;
                    float[] leftHandPositionY = { start, start - 0.03f, start };
                    clip.SetCurve(part.name, typeof(Transform), "localPosition.y", CreateCurve(0.5f, leftHandPositionY));
                    float[] leftHandPositionX = { -0.39f, -0.39f, -0.39f };
                    clip.SetCurve(part.name, typeof(Transform), "localPosition.x", CreateCurve(0.5f, leftHandPositionX));
                } else if (part.name == "rightHand") {
                    start = part.localPosition.y;
                    float[] rightHandPositionY = { start, start - 0.03f, start };
                    clip.SetCurve(part.name, typeof(Transform), "localPosition.y", CreateCurve(0.5f, rightHandPositionY));
                    float[] rightHandPositionX = { 0.44f, 0.44f, 0.44f };
                    clip.SetCurve(part.name, typeof(Transform), "localPosition.x", CreateCurve(0.5f, rightHandPositionX));
                } else if (part.name == "leftFoot") {
                    SetRotationOnClip(clip, part, 0f, true);
                } else if (part.name == "rightFoot") {
                    SetRotationOnClip(clip, part, 0f, true);
                }
            }
            clip.legacy = true;
            clip.wrapMode = WrapMode.Loop;
            animation.AddClip (clip, clip.name);
		}

		public static void SetMoveAnimation(Animation animation, Transform[] transforms) {
			AnimationClip clip = new AnimationClip ();
            clip.name = "Move";

			float start = 0f;
			foreach (Transform part in transforms) {
				if (part.name == "body") {
                    SetRotationOnClip(clip, part, 0.03f);
                } else if (part.name == "head") {
					start = part.localPosition.y;
					float[] headPosition = { start, start - .01f, start, start + .01f, start };
                    clip.SetCurve(part.name, typeof(Transform), "localPosition.y", CreateCurve(0.1f, headPosition));
				} else if (part.name == "leftHand") {
					start = part.localPosition.x;
					float[] leftHandPosition = { start, start - .06f, start, start + .06f, start };
                    clip.SetCurve(part.name, typeof(Transform), "localPosition.x", CreateCurve(0.1f, leftHandPosition));
				} else if (part.name == "rightHand") {
					start = part.localPosition.x;
					float[] rightHandPosition = { start, start + .06f, start, start - .06f, start };
                    clip.SetCurve(part.name, typeof(Transform), "localPosition.x", CreateCurve(0.1f, rightHandPosition));
				} else if (part.name == "leftFoot") {
					SetRotationOnClip(clip, part, -0.12f);
				} else if (part.name == "rightFoot") {
                    SetRotationOnClip(clip, part, 0.12f);
                }
            }

            clip.legacy = true;
            clip.wrapMode = WrapMode.Loop;
			animation.AddClip (clip, clip.name);
		}

        private static void SetRotationOnClip(AnimationClip clip, Transform part, float delta, bool reset = false) {
            Quaternion angle = Quaternion.Euler(0, 0, delta);
            float[] x = { angle.x, angle.x, angle.x, angle.x };
            clip.SetCurve(part.name, typeof(Transform), "localRotation.x", CreateCurve(0.1f, x));
            float[] y = { angle.y, angle.y, angle.y, angle.y };
            clip.SetCurve(part.name, typeof(Transform), "localRotation.y", CreateCurve(0.1f, y));
            float[] w = { angle.w, angle.w, angle.w, angle.w };
            clip.SetCurve(part.name, typeof(Transform), "localRotation.w", CreateCurve(0.1f, w));
            float start = reset ? 0f : part.localRotation.z;
            float[] rightFootRotation = { start, start - delta, start, start + delta, start };
            clip.SetCurve(part.name, typeof(Transform), "localRotation.z", CreateCurve(0.1f, rightFootRotation));
        }
    }
}

