using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cocos2D
{
    public class CCDrawNode : CCNode
    {
        const int DefaultBufferSize = 512;

        private CCRawList<VertexPositionColor> m_pVertices;
        private CCBlendFunc m_sBlendFunc;
        private bool m_bDirty;

        public CCDrawNode()
        {
            Init();
        }

        public CCBlendFunc BlendFunc
        {
            get { return m_sBlendFunc; }
            set { m_sBlendFunc = value; }
        }

        public override bool Init()
        {
            base.Init();

            m_sBlendFunc = CCBlendFunc.AlphaBlend;
            m_pVertices = new CCRawList<VertexPositionColor>(DefaultBufferSize);
            return true;
        }

        /** draw a dot at a position, with a given radius and color */

        public void DrawDot(CCPoint pos, float radius, CCColor4F color)
        {
            var cl = new Color(color.R, color.G, color.B, color.A);

            var a = new VertexPositionColor(new Vector3(pos.X - radius, pos.Y - radius, 0), cl); //{-1.0, -1.0}
            var b = new VertexPositionColor(new Vector3(pos.X - radius, pos.Y + radius, 0), cl); //{-1.0,  1.0}
            var c = new VertexPositionColor(new Vector3(pos.X + radius, pos.Y + radius, 0), cl); //{ 1.0,  1.0}
            var d = new VertexPositionColor(new Vector3(pos.X + radius, pos.Y - radius, 0), cl); //{ 1.0, -1.0}

            m_pVertices.Add(a);
            m_pVertices.Add(b);
            m_pVertices.Add(c);

            m_pVertices.Add(a);
            m_pVertices.Add(c);
            m_pVertices.Add(d);

            m_bDirty = true;
        }
        
        /// <summary>
        /// Creates 18 vertices that create a segment between the two points with the given radius of rounding
        /// on the segment end. The color is used to draw the segment.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        /// <returns>The starting vertex index of the segment.</returns>
        public virtual int DrawSegment(CCPoint from, CCPoint to, float radius, CCColor4F color)
        {
            var cl = new Color(color.R, color.G, color.B, color.A);

            var a = from;
            var b = to;

            var n = CCPoint.Normalize(CCPoint.Perp(a - b));
            var t = CCPoint.Perp(n);

            var nw = n * radius;
            var tw = t * radius;
            var v0 = b - (nw + tw);
            var v1 = b + (nw - tw);
            var v2 = b - nw;
            var v3 = b + nw;
            var v4 = a - nw;
            var v5 = a + nw;
            var v6 = a - (nw - tw);
            var v7 = a + (nw + tw);

            int returnIndex = m_pVertices.Count;
            m_pVertices.Add(new VertexPositionColor(v0, cl)); //__t(v2fneg(v2fadd(n, t)))
            m_pVertices.Add(new VertexPositionColor(v1, cl)); //__t(v2fsub(n, t))
            m_pVertices.Add(new VertexPositionColor(v2, cl)); //__t(v2fneg(n))}

            m_pVertices.Add(new VertexPositionColor(v3, cl)); //__t(n)
            m_pVertices.Add(new VertexPositionColor(v1, cl)); //__t(v2fsub(n, t))
            m_pVertices.Add(new VertexPositionColor(v2, cl)); //__t(v2fneg(n))

            m_pVertices.Add(new VertexPositionColor(v3, cl)); //__t(n)
            m_pVertices.Add(new VertexPositionColor(v4, cl)); //__t(v2fneg(n))
            m_pVertices.Add(new VertexPositionColor(v2, cl)); //__t(v2fneg(n))

            m_pVertices.Add(new VertexPositionColor(v3, cl)); //__t(n)
            m_pVertices.Add(new VertexPositionColor(v4, cl)); //__t(v2fneg(n))
            m_pVertices.Add(new VertexPositionColor(v5, cl)); //__t(n)

            m_pVertices.Add(new VertexPositionColor(v6, cl)); //__t(v2fsub(t, n))
            m_pVertices.Add(new VertexPositionColor(v4, cl)); //__t(v2fneg(n))
            m_pVertices.Add(new VertexPositionColor(v5, cl)); //__t(n)

            m_pVertices.Add(new VertexPositionColor(v6, cl)); //__t(v2fsub(t, n))
            m_pVertices.Add(new VertexPositionColor(v7, cl)); //__t(v2fadd(n, t))
            m_pVertices.Add(new VertexPositionColor(v5, cl)); //__t(n)

            m_bDirty = true;
            return (returnIndex);
        }

        public virtual void FadeBySegment(int vertexStart, float fadeFactor) 
        {
            FadeByVertices(vertexStart, 18, fadeFactor);
        }

        public virtual void FadeToSegment(int vertexStart, float fadeFactor)
        {
            FadeToVertices(vertexStart, 18, fadeFactor);
        }

        public virtual void RemoveSegment(int vertexStart) 
        {
            if (m_pVertices.Count == 18)
            {
                m_pVertices.Clear();
            }
            else
            {
                m_pVertices.RemoveRange(vertexStart, 18);
            }
            m_bDirty = true;
        }

        /// <summary>
        /// Multiplicatively applies the fadeFactor to the alpha channel of the vertices starting
        /// with start and for the number of vertices defined by count. the alpha channel is
        /// determined by the current alpha * fadeFactor.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="fadeFactor"></param>
        public virtual void FadeByVertices(int start, int count, float fadeFactor)
        {
            for (int i = 0; i < count; i++)
            {
                VertexPositionColor vpc = m_pVertices[start + i];
                Color c = vpc.Color;
                vpc.Color = new Color(c.R, c.G, c.B, (byte)(c.A * fadeFactor));
                m_pVertices[start + i] = vpc;
            }
            m_bDirty = true;
        }

        /// <summary>
        /// For the start and count vertices drawn, this will set the alpha channel to the given fade factor.
        /// The alpha is determined by 255 * fadeFactor.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="fadeFactor"></param>
        public virtual void FadeToVertices(int start, int count, float fadeFactor)
        {
            for (int i = 0; i < count; i++)
            {
                VertexPositionColor vpc = m_pVertices[start + i];
                Color c = vpc.Color;
                vpc.Color = new Color(c.R, c.G, c.B, (byte)(255f * fadeFactor));
                m_pVertices[start + i] = vpc;
            }
            m_bDirty = true;
        }

        /** draw a polygon with a fill color and line color */

        private struct ExtrudeVerts
        {
            public CCPoint offset;
            public CCPoint n;
        }

        public void DrawCircleOutline(CCPoint center, float radius, float lineWidth, CCColor4B color)
        {
            DrawCircleOutline(center, radius, lineWidth, CCMacros.CCDegreesToRadians(360f), 360, color);
        }

        public void DrawCircleOutline(CCPoint center, float radius, float lineWidth, float angle, int segments, CCColor4B color)
        {
            float increment = MathHelper.Pi * 2.0f / segments;
            double theta = 0.0;

            CCPoint v1;
            CCPoint v2 = CCPoint.Zero;
            CCColor4F cf = new CCColor4F(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

            for (int i = 0; i < segments; i++)
            {
                v1 = center + new CCPoint((float)Math.Cos(theta), (float)Math.Sin(theta)) * radius;
                v2 = center + new CCPoint((float)Math.Cos(theta + increment), (float)Math.Sin(theta + increment)) * radius;
                DrawSegment(v1, v2, lineWidth, cf);
                theta += increment;
            }
        }

        public void DrawCircle(CCPoint center, float radius, CCColor4B color)
        {
            DrawCircle(center, radius, CCMacros.CCDegreesToRadians(360f), 360, color);
        }


        public void DrawCircle(CCPoint center, float radius, float angle, int segments, CCColor4B color)
        {
            float increment = MathHelper.Pi * 2.0f / segments;
            double theta = 0.0;

            CCPoint v1;
            CCPoint v2 = CCPoint.Zero;
            List<CCPoint> verts = new List<CCPoint>();

            for (int i = 0; i < segments; i++)
            {
                v1 = center + new CCPoint((float)Math.Cos(theta), (float)Math.Sin(theta)) * radius;
                v2 = center + new CCPoint((float)Math.Cos(theta + increment), (float)Math.Sin(theta + increment)) * radius;
                verts.Add(v1);
                theta += increment;
            }
            CCColor4F cf = new CCColor4F(color.R/255f, color.G/255f, color.B/255f, color.A/255f);
            DrawPolygon(verts.ToArray(), verts.Count, cf, 0, new CCColor4F(0f, 0f, 0f, 0f));
        }

        public void DrawRect(CCRect rect, CCColor4B color)
        {
            float x1 = rect.MinX;
            float y1 = rect.MinY;
            float x2 = rect.MaxX;
            float y2 = rect.MaxY;
            CCPoint[] pt = new CCPoint[] { 
                new CCPoint(x1,y1), new CCPoint(x2,y1), new CCPoint(x2,y2), new CCPoint(x1,y2)
            };
            CCColor4F cf = new CCColor4F(color.R/255f, color.G/255f, color.B/255f, color.A/255f);
            DrawPolygon(pt, 4, cf, 0, new CCColor4F(0f, 0f, 0f, 0f));
        }

		public void DrawRect(CCRect rect, CCColor4F color, float borderWidth, CCColor4F borderColor)
		{
			float x1 = rect.MinX;
			float y1 = rect.MinY;
			float x2 = rect.MaxX;
			float y2 = rect.MaxY;
			CCPoint[] pt = new CCPoint[] { 
				new CCPoint(x1,y1), new CCPoint(x2,y1), new CCPoint(x2,y2), new CCPoint(x1,y2)
			};
			DrawPolygon(pt, 4, color, borderWidth, borderColor);
		}

        public void DrawPolygon(CCPoint[] verts, int count, CCColor4F fillColor, float borderWidth,
                                CCColor4F borderColor)
        {
            var extrude = new ExtrudeVerts[count];

            for (int i = 0; i < count; i++)
            {
                var v0 = verts[(i - 1 + count) % count];
                var v1 = verts[i];
                var v2 = verts[(i + 1) % count];

                var n1 = CCPoint.Normalize(CCPoint.Perp(v1 - v0));
                var n2 = CCPoint.Normalize(CCPoint.Perp(v2 - v1));

                var offset = (n1 + n2) * (1.0f / (CCPoint.Dot(n1, n2) + 1.0f));
                extrude[i] = new ExtrudeVerts() {offset = offset, n = n2};
            }

            bool outline = (fillColor.A > 0.0f && borderWidth > 0.0f);

            float inset = (!outline ? 0.5f : 0.0f);
            
            for (int i = 0; i < count - 2; i++)
            {
                var v0 = verts[0] - (extrude[0].offset * inset);
                var v1 = verts[i + 1] - (extrude[i + 1].offset * inset);
                var v2 = verts[i + 2] - (extrude[i + 2].offset * inset);

                m_pVertices.Add(new VertexPositionColor(v0, fillColor)); //__t(v2fzero)
                m_pVertices.Add(new VertexPositionColor(v1, fillColor)); //__t(v2fzero)
                m_pVertices.Add(new VertexPositionColor(v2, fillColor)); //__t(v2fzero)
            }

            for (int i = 0; i < count; i++)
            {
                int j = (i + 1) % count;
                var v0 = verts[i];
                var v1 = verts[j];

                var n0 = extrude[i].n;

                var offset0 = extrude[i].offset;
                var offset1 = extrude[j].offset;

                if (outline)
                {
                    var inner0 = (v0 - (offset0 * borderWidth));
                    var inner1 = (v1 - (offset1 * borderWidth));
                    var outer0 = (v0 + (offset0 * borderWidth));
                    var outer1 = (v1 + (offset1 * borderWidth));

                    m_pVertices.Add(new VertexPositionColor(inner0, borderColor)); //__t(v2fneg(n0))
                    m_pVertices.Add(new VertexPositionColor(inner1, borderColor)); //__t(v2fneg(n0))
                    m_pVertices.Add(new VertexPositionColor(outer1, borderColor)); //__t(n0)

                    m_pVertices.Add(new VertexPositionColor(inner0, borderColor)); //__t(v2fneg(n0))
                    m_pVertices.Add(new VertexPositionColor(outer0, borderColor)); //__t(n0)
                    m_pVertices.Add(new VertexPositionColor(outer1, borderColor)); //__t(n0)
                }
                else
                {
                    var inner0 = (v0 - (offset0 * 0.5f));
                    var inner1 = (v1 - (offset1 * 0.5f));
                    var outer0 = (v0 + (offset0 * 0.5f));
                    var outer1 = (v1 + (offset1 * 0.5f));

                    m_pVertices.Add(new VertexPositionColor(inner0, fillColor)); //__t(v2fzero)
                    m_pVertices.Add(new VertexPositionColor(inner1, fillColor)); //__t(v2fzero)
                    m_pVertices.Add(new VertexPositionColor(outer1, fillColor)); //__t(n0)

                    m_pVertices.Add(new VertexPositionColor(inner0, fillColor)); //__t(v2fzero)
                    m_pVertices.Add(new VertexPositionColor(outer0, fillColor)); //__t(n0)
                    m_pVertices.Add(new VertexPositionColor(outer1, fillColor)); //__t(n0)
                }
            }
            m_bDirty = true;
        }

        public void DrawLine(CCPoint from, CCPoint to, float lineWidth = 1, CCLineCap lineCap = CCLineCap.Butt)
        {
            DrawLine(from, to, lineWidth, new CCColor4B(Color.R, Color.G, Color.B, Opacity));
        }
        public void DrawLine(CCPoint from, CCPoint to, CCColor4B color, CCLineCap lineCap = CCLineCap.Butt)
        {
            DrawLine(from, to, 1, color);
        }

        public void DrawLine(CCPoint from, CCPoint to, float lineWidth, CCColor4B color, CCLineCap lineCap = CCLineCap.Butt)
        {
            System.Diagnostics.Debug.Assert(lineWidth >= 0, "Invalid value specified for lineWidth : value is negative");
            if (lineWidth <= 0)
                return;

            var cl = color;

            var a = from;
            var b = to;

            var normal = CCPoint.Normalize(a - b);
            if (lineCap == CCLineCap.Square)
            {
                var nr = normal * lineWidth;
                a += nr;
                b -= nr;
            }

            var n = CCPoint.PerpendicularCounterClockwise(normal);

            var nw = n * lineWidth;
            var v0 = b - nw;
            var v1 = b + nw;
            var v2 = a - nw;
            var v3 = a + nw;

            // Triangles from beginning to end
            m_pVertices.Add(new VertexPositionColor(v1, cl));
            m_pVertices.Add(new VertexPositionColor(v2, cl));
            m_pVertices.Add(new VertexPositionColor(v0, cl));

            m_pVertices.Add(new VertexPositionColor(v1, cl));
            m_pVertices.Add(new VertexPositionColor(v2, cl));
            m_pVertices.Add(new VertexPositionColor(v3, cl));

            if (lineCap == CCLineCap.Round)
            {
                var mb = (float)Math.Atan2(v1.Y - b.Y, v1.X - b.X);
                var ma = (float)Math.Atan2(v2.Y - a.Y, v2.X - a.X);

                // Draw rounded line caps
                DrawSolidArc(a, lineWidth, -ma, -MathHelper.Pi, color);
                DrawSolidArc(b, lineWidth, -mb, -MathHelper.Pi, color);
            }

            m_bDirty = true;
        }

        // Used for drawing line caps
        public void DrawSolidArc(CCPoint pos, float radius, float startAngle, float sweepAngle, CCColor4B color)
        {
            var cl = color;

            int segments = (int)(10 * (float)Math.Sqrt(radius));  //<- Let's try to guess at # segments for a reasonable smoothness

            float theta = -sweepAngle / (segments - 1);// MathHelper.Pi * 2.0f / segments;
            float tangetial_factor = (float)Math.Tan(theta);   //calculate the tangential factor 

            float radial_factor = (float)Math.Cos(theta);   //calculate the radial factor 

            float x = radius * (float)Math.Cos(-startAngle);   //we now start at the start angle
            float y = radius * (float)Math.Sin(-startAngle);

            var verticeCenter = new CCV3F_C4B(pos, cl);
            var vert1 = new CCV3F_C4B(CCVertex3F.Zero, cl);
            float tx = 0;
            float ty = 0;

            for (int i = 0; i < segments - 1; i++)
            {
                m_pVertices.Add(new VertexPositionColor(pos, cl));

                vert1.Vertices.X = x + pos.X;
                vert1.Vertices.Y = y + pos.Y;
                m_pVertices.Add(new VertexPositionColor(new Vector3(vert1.Vertices.X, vert1.Vertices.Y, vert1.Vertices.Z), cl));

                //calculate the tangential vector 
                //remember, the radial vector is (x, y) 
                //to get the tangential vector we flip those coordinates and negate one of them 
                tx = -y;
                ty = x;

                //add the tangential vector 
                x += tx * tangetial_factor;
                y += ty * tangetial_factor;

                //correct using the radial factor 
                x *= radial_factor;
                y *= radial_factor;

                vert1.Vertices.X = x + pos.X;
                vert1.Vertices.Y = y + pos.Y;

                m_pVertices.Add(new VertexPositionColor(new Vector3(vert1.Vertices.X, vert1.Vertices.Y, 0), cl));
            }

            m_bDirty = true;
        }

        /** Clear the geometry in the node's buffer. */

        public virtual void Clear()
        {
            m_pVertices = new CCRawList<VertexPositionColor>(DefaultBufferSize);
            m_bDirty = true;
            _toDraw = null;
            base.ContentSize = CCSize.Zero;
        }

        public bool FilterPrimitivesByAlpha
        {
            get;
            set;
        }

        private VertexPositionColor[] _toDraw;

        public override void Draw()
        {
            if (m_bDirty)
            {
                m_bDirty = false;
                if (FilterPrimitivesByAlpha)
                {
                    _toDraw = m_pVertices.Elements.Where(x => x.Color.A > 0).ToArray();
                }
                else
                {
                    _toDraw = m_pVertices.Elements;
                }
            }

            if (_toDraw != null)
            {
                CCDrawManager.TextureEnabled = false;
                CCDrawManager.BlendFunc(m_sBlendFunc);
                CCDrawManager.DrawPrimitives(PrimitiveType.TriangleList, _toDraw, 0, _toDraw.Length / 3);
            }
        }
    }
}
