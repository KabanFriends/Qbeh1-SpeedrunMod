using System;
using System.Collections.Generic;
using UnityEngine;

namespace Qbeh1_SpeedrunMod.Common
{
    public class LabelRenderer
    {
        private const float LABEL_WIDTH = 500f;

        public List<object> labels = new List<object>();
        public TextAnchor alignment = TextAnchor.UpperLeft;

        public GUISkin skin;
        public bool drawShadow;

        private float x;
        private float y;

        public LabelRenderer(float x, float y, List<object> textList = null)
        {
            if (textList != null)
            {
                this.labels = textList;
            }

            drawShadow = true;
            this.x = x;
            this.y = y;
        }

        public void Draw()
        {
            if (skin == null)
            {
                return;
            }

            GUIStyle style = new GUIStyle();
            style.alignment = alignment;

            var rx = x;

            if (alignment == TextAnchor.UpperRight || alignment == TextAnchor.LowerRight || alignment == TextAnchor.MiddleRight)
            {
                rx = x - LABEL_WIDTH;
            }

            GUI.skin = skin;

            float cy = y;
            foreach (object obj in labels)
            {
                if (obj is string s)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        if (drawShadow)
                        {
                            style.normal.textColor = Color.black;

                            Rect shadowRect = new Rect(rx + 1, cy + 1, LABEL_WIDTH, 30f);
                            GUI.Label(shadowRect, s, style);
                        }

                        style.normal.textColor = new Color(1f, 0.995f, 0.816f, 1f);

                        Rect rect = new Rect(rx, cy, LABEL_WIDTH, 30f);
                        GUI.Label(rect, s, style);
                    }
                } else if (obj is ColoredLabel label)
                {
                    if (!string.IsNullOrEmpty(label.text))
                    {
                        if (drawShadow)
                        {
                            style.normal.textColor = Color.black;

                            Rect shadowRect = new Rect(rx + 1, cy + 1, LABEL_WIDTH, 30f);
                            GUI.Label(shadowRect, label.text, style);
                        }

                        style.normal.textColor = label.color;

                        Rect rect = new Rect(rx, cy, LABEL_WIDTH, 30f);
                        GUI.Label(rect, label.text, style);
                    }
                } else
                {
                    throw new ArgumentException("Label is not string or ColoredLabel.");
                }

                cy += 15f;
            }
        }
    }
}
