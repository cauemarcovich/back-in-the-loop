using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Helpers {
    public static class ExtensionMethods {
        public static bool IsBetween (this int number, int min, int max) {
            return number >= min && number <= max;
        }

        public static bool IsBetween (this float number, float min, float max) {
            return number >= min && number <= max;
        }

        public static List<Transform> GetAllElements (this Transform parent) {
            var list = new List<Transform> () { parent };

            if (parent.childCount > 0) {
                foreach (Transform child in parent) {
                    var childList = GetAllElements (child);
                    list.AddRange (childList);
                }
            }

            return list;
        }

        public static bool HasComponent<T> (this GameObject gameObject) {
            var component = gameObject.GetComponent<T> ();
            return component != null && !component.Equals (null);
        }
    }
}