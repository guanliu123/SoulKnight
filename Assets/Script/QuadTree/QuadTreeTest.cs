/*
using System.Collections.Generic;
using OPH.Collision.QuadTree;
using TMPro;
using UnityEngine;
using uEvent = UnityEngine.Event;

namespace OPH.Example {
    public class QuadTreeTest : MonoBehaviour {
        QTree<Element> TreeRoot;
        public Camera cam;
        public Element element;
        [HideInInspector] public Vector2 size;
        public Element myCube;
        public TextMeshProUGUI amount;
        public TextMeshProUGUI build;
        public TextMeshProUGUI click;
        [Header("所有元素")]
        public List<Element> eleList = new List<Element>();
        [Header("Draw")]
        public bool IsShow = false;
        List<QTree<Element>> qtList;
        List<Element> clickList;
        private int idx = 0;

        protected float buildTime;
        protected int Count;
        protected int MaxCount = 60;

        protected float clickTime;

        private void Awake() {
            cam.ResetAspect();
            float wigth = 2 * cam.orthographicSize * cam.aspect;
            wigth = Mathf.FloorToInt(wigth);
            size.x = size.y = wigth;
            TreeRoot = QTree<Element>.CreateRoot(4,6).InitRect(0, 0, size.x, size.y);
            qtList = new List<QTree<Element>>();
            clickList = new List<Element>();
            idx = 0;
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                CreateElemnt();
                amount.text = eleList.Count.ToString("000000");
            }
            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                for (int i = 0; i < 100; ++i) {
                    CreateElemnt();
                }
                amount.text = eleList.Count.ToString("000000");
            }
            if (Input.GetKeyDown(KeyCode.Alpha3)) {
                for (int i = 0; i < 5000; ++i) {
                    CreateElemnt();
                }
                amount.text = eleList.Count.ToString("000000");
            }
            TreeUpdate();
        }

        private void FixedUpdate() {
            
        }

        private void TreeUpdate() {
            TreeRoot.Clear();
            qtList.Clear();
            for (int i = 0; i < clickList.Count; ++i) {
                clickList[i].Click(false);
            }
            clickList.Clear();

            float t1 = Time.realtimeSinceStartup;
            for (int i = 0; i < eleList.Count; ++i) {
                TreeRoot.Insert(eleList[i]);
            }
            float t2 = Time.realtimeSinceStartup;

            if (IsShow) {
                TreeRoot.GetAllChildNodes(ref qtList);
                for (int i = 0; i < qtList.Count; ++i) {
                    float halfX = qtList[i].Width / 2f;
                    float halfY = qtList[i].Height / 2f;
                    Vector3[] point = new Vector3[4];
                    point[0].x = point[1].x = point[2].x = point[3].x = qtList[i].X;
                    point[0].y = point[1].y = point[2].y = point[3].y = qtList[i].Y;

                    point[0].x -= halfX; point[0].y -= halfY;
                    point[1].x -= halfX; point[1].y += halfY;
                    point[2].x += halfX; point[2].y += halfY;
                    point[3].x += halfX; point[3].y -= halfY;

                    DrawRect(point);
                }
            }

            float t3, t4;
            t3 = t4 = 0;
            if (myCube) {
                t3 = Time.realtimeSinceStartup;
                
                //TreeRoot.GetAroundObj(ref myCube, ref clickList);
                t4 = Time.realtimeSinceStartup;
                for (int i = 0; i < clickList.Count; ++i) {
                    clickList[i].Click(true);
                }
            }



            buildTime += t2 - t1;
            clickTime += (t4 - t3) * 1000;
            ++Count;
            if (Count >= MaxCount) {
                build.text = $"{buildTime / MaxCount:0.00000} s";
                click.text = $"{clickTime / MaxCount:0.00000} ms";
                Count = 0;
                buildTime = 0;
                clickTime = 0;
            }
        }

        public void CreateElemnt() {
            Element ele = Instantiate(element, Vector3.zero, Quaternion.identity);
            ele.name = $"Ele {idx++}";
            ele.Init(size);
            eleList.Add(ele);
        }

        private void DrawRect(Vector3[] points) {
            for (int i = 0; i < points.Length; i++) {
                //Gizmos.DrawLine(points[i], points[(i + 1) % points.Length]);
                Debug.DrawLine(points[i], points[(i + 1) % points.Length], Color.white, 0.02f);
            }
        }


        #region IMGUI
        protected Rect buttonRect = new Rect(0, 60, 100, 50);
        protected bool IsDraging = false;
        protected Vector2 dragOrigin;

        private void OnGUI() {
            switch (uEvent.current.type) {
            case EventType.MouseDown:
                if (buttonRect.Contains(uEvent.current.mousePosition)) {
                    IsDraging = true;
                    dragOrigin = buttonRect.position - uEvent.current.mousePosition;
                }
                break;
            case EventType.MouseUp:
                IsDraging = false;
                break;
            default:
                break;
            }

            if (IsDraging) {
                Vector2 mousePos = uEvent.current.mousePosition;
                buttonRect.position = (mousePos + dragOrigin);
            }

            if (GUI.Button(buttonRect, IsShow ? "Hide" : "Show")) {
                IsShow = !IsShow;
            }
        }
        #endregion
    }
}
*/
