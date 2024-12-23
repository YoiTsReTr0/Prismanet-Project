using System;
using System.Collections.Generic;
using UnityEngine;

namespace MainGame.ConnectDots
{
    public class Connect_PlacementObj : MonoBehaviour
    {
        public List<RectTransform> ChildrenList = new();
        public Queue<RectTransform> AvailableChildren = new();

        private void Start()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                ChildrenList.Add(transform.GetChild(i).GetComponent<RectTransform>());
                AvailableChildren.Enqueue(ChildrenList[i]);
            }
        }

        public void ResetChildrenAndDespawn()
        {
            AvailableChildren.Clear();

            foreach (var child in ChildrenList)
            {
                for (int i = 0; i < child.childCount; i++)
                {
                    Destroy(child.GetChild(i).gameObject);
                }

                AvailableChildren.Enqueue(child);
            }
        }
    }
}