﻿using LyricsExtractor.Components.Editors;
using LyricsExtractor.Services;
using SkiaSharp;
using System.Drawing;

namespace SubtitleEditor.SectionDef
{
    public class SeekBarClip : Clip
    {
        public SeekBarClip(double start):base(start, start)
        {

        }
		public override async Task OnPaintBeforeAsync(int layerIndex, int layersCount, double min, double max, int Width, int Height, Graphics g, double bMin, double bMax)
		{
            try
            {
                float bX = (float)(Start - bMin) / (float)(bMax - bMin) * Width;
                float x = (float)(Start - min) / (float)(max - min) * Width;
                double h1 = ZoomBarHeight * 2, h2 = Height - sbh, h3 = Height;
                Color c = Color.FromArgb(120, 0, 0, 0);
                Color cp = c;
                Color cf = Color.FromArgb(80, 30, 160, 40);
                if (hoverOver == 0)
                {
                    cf = Color.FromArgb(140, 30, 160, 40);
                    c = Color.FromArgb(230, 0, 0, 0);
                    cp = c;
                }
                if (HeldComp == 0)
                {
                    cf = Color.FromArgb(160, 9, 159, 232);
                    c = Color.FromArgb(120, 255, 255, 255);
                    cp = Color.FromArgb(255, 0, 0, 0);
                }
                g.FillPolygon(cp, new PointF[]{
                    new PointF(x, Height - sbh),
                    new PointF(x - sbh/2, Height),
                    new PointF (x+sbh/2, Height)});

                // The seekbar
                g.DrawRectangle(cp, 1, x - 10, Height - sbh, 20, sbh, 5);
                g.FillRectangle(cf, x - 10, Height - sbh, 20, sbh, 5);
                g.DrawLine(cf, 4, x, ZoomBarHeight * 2, x, Height - ZoomBarHeight);
                g.DrawLine(cp, 2, x, ZoomBarHeight * 2, x, Height - ZoomBarHeight);


                g.DrawPolygon(cp, 1F, new PointF[]{
                    new PointF(bX,2*ZoomBarHeight),
                    new PointF(bX - ZoomBarHeight/2, ZoomBarHeight),
                    new PointF (bX+ZoomBarHeight/2, ZoomBarHeight)});

            }
            catch { }
		}

		public override Cursor MouseMove(int layerIndex, int layersCount, Point e, double max, double min, int Width, int Height, Cursor c, InvalidateRef method, double bMin, double bigM)
		{
			int eToSs = (int)Math.Round(e.X * (max - min) / Width + min);
			int sToE = (int)Math.Round(((double)Start - min) / (max - min) * Width);
			int eToE = (int)Math.Round(((double)End - min) / (max - min) * Width);

            //determine where we are hovering
            // 0 is center hover, -1, left edge hover, +1 is right edge hover, -2 is left section out, 2 is right section out, 3 is out of region
            // seek bar cannot be -1 or 1
            if (HeldComp < -1 || HeldComp > 1)
            {
                int inTol = 4, outTol = 4;

                // determine a hover section. dont consider y position yet
                if (e.X >= sToE - 13 && e.X <= sToE + 13)
                { c = Cursors.NoMoveHoriz; hoverOver = 0; }
                else
                {
                    if (e.X < sToE - 10)
                        hoverOver = -2;
                    else if (e.X > sToE + 10)
                        hoverOver = 2;
                    c = Cursors.Default;
                }
                if (e.Y < Height - sbh)
                {
                    c = Cursors.Default;
                    hoverOver = 3;
                }

            }

			// already held
			if (HeldComp >= -1 && HeldComp <= 1)
			{
				// for length changing bars

				double minT = 1 / 30.0; // 30fps
				int movE = e.X - mDownOn.X;

				double mov = Math.Round(movE * ((double)max - min) / Width, 2);
                //seek bar cannot be -1 or 1, its 0, -2, 2, or 3
                if (HeldComp == 0) // draging
                {
                    if (mDownSec.Start + mov < bMin) // too left
                        mov = bMin - mDownSec.Start;
                    else if (mDownSec.Start + mov > bigM) // too right
                        mov = bigM - mDownSec.Start;

                    Start = mDownSec.Start + mov;
                }
                else if (HeldComp == -1) // not the case with seekbar
                {
                    if (mov < 0)//  increasing length to left
                    {
                        if (mDownSec.Start + mov < bMin) // too left
                            mov = bMin - mDownSec.Start;
                    }
                    else
                    {
                        if (mDownSec.End - mDownSec.Start - mov < minT) // less than X sec
                            mov = mDownSec.End - minT - mDownSec.Start;
                    }
                    Start = mDownSec.Start + mov;
                }
                else if (HeldComp == 1)
                {
                    if (mov > 0)//  increasing length to right
                    {
                        if (mDownSec.End + mov > bigM) // too right
                            mov = bigM - mDownSec.End;
                    }
                    else
                    {
                        if (mDownSec.End - mDownSec.Start + mov < minT) // less than X sec
                            mov = -mDownSec.End + minT + mDownSec.Start;
                    }
                    End = mDownSec.End + mov;
                }

			}

			method();
			curLoc = e;
			return c;
		}
	}
	public class ZoomBarClip : Clip
	{
		public ZoomBarClip(double start, double end) : base(start, end)
		{

		}
        //some items need to be painted the grid is painted.
        public override async Task OnPaintBeforeAsync(int layerIndex, int layersCount, double min, double max, int Width, int Height, Graphics g, double bMin, double bMax)
        {
            try
            {
                //calculate the rectangle.
                // for zoom section, min is always 0. max is equal to the length of the trimBar
                // Width is not same as max. Width is the graphical length.
                zsRec = new Rectangle(
                      (int)Math.Round(((double)Start - min) / (max - min) * Width),
                      0,
                      (int)Math.Round(((double)End - Start) / (max - min) * Width),
                      ZoomBarHeight);
                Color c1 = Color.FromArgb(180, 20, 20, 20);
                Color c2 = Color.FromArgb(255, 120, 120, 120);
                Color c3 = c1;
                int edgeWidth = ZoomBarHeight / 2;
                if (hoverOver == -1)
                    c1 = Color.FromArgb(180, 80, 80, 80);
                if (hoverOver == 0)
                    c2 = Color.FromArgb(255, 160, 160, 160);
                if (hoverOver == 1)
                    c3 = Color.FromArgb(180, 60, 60, 60);
                if (HeldComp == -1)
                    c1 = Color.FromArgb(180, 200, 200, 200);
                if (HeldComp == 0)
                    c2 = Color.FromArgb(255, 200, 200, 200);
                if (HeldComp == 1)
                    c3 = Color.FromArgb(180, 200, 200, 200);
                g.FillRectangle(Color.FromArgb(240, 240, 240), 0, 0, Width, zsRec.Height);
                g.FillEllipse(
                    c1,
                    zsRec.X, zsRec.Y, 2 * edgeWidth, zsRec.Height
                    );
                g.FillEllipse(
                    c3,
                    zsRec.X + zsRec.Width - edgeWidth * 2, zsRec.Y, edgeWidth * 2, zsRec.Height
                    );
                g.FillRectangle(
                    c2,
                    zsRec.X + edgeWidth, zsRec.Y, zsRec.Width - edgeWidth * 2, zsRec.Height
                    );
                zsRec = new Rectangle();
            }
            catch { }
            //g.FillRectangle((Brush)new SolidBrush(Color.FromArgb(30, 0, 255, 0)), zsRec);
        }

		public override Cursor MouseMove(int layerIndex, int layersCount, Point e, double max, double min, int Width, int Height, Cursor c, InvalidateRef method, double bMin, double bigM)
		{
			int eToSs = (int)Math.Round(e.X * (max - min) / Width + min);
			int sToE = (int)Math.Round(((double)Start - min) / (max - min) * Width);
			int eToE = (int)Math.Round(((double)End - min) / (max - min) * Width);

			//determine where we are hovering
			// 0 is center hover, -1, left edge hover, +1 is right edge hover, -2 is left section out, 2 is right section out, 3 is out of region
			// seek bar cannot be -1 or 1
			if (HeldComp < -1 || HeldComp > 1)
			{
				int inTol = 4, outTol = 4;

					inTol = ZoomBarHeight / 2;
					// determine a hover section. dont consider y position yet
					if (e.X >= sToE - outTol && e.X <= sToE + inTol)
					{
						c = Cursors.VSplit; hoverOver = -1;
					}
					else if (e.X >= eToE - inTol && e.X <= eToE + outTol)
					{ c = Cursors.VSplit; hoverOver = 1; }
					else if (e.X >= sToE && e.X <= eToE)
					{ c = Cursors.NoMoveHoriz; hoverOver = 0; }
					else
					{
						if (eToSs < Start)
							hoverOver = -2;
						else
							hoverOver = 2;
						c = Cursors.Default;
					}
					if (e.Y > ZoomBarHeight)
					{
						c = Cursors.Default;
						hoverOver = 3;
					}
			}

            // already held
            if (HeldComp >= -1 && HeldComp <= 1)
            {
                // for length changing bars

                double minT = 1 / 30.0; // 30fps
                int movE = e.X - mDownOn.X;

                double mov = Math.Round(movE * ((double)max - min) / Width, 2);
                //seek bar cannot be -1 or 1, its 0, -2, 2, or 3
                if (HeldComp == 0) // draging
                {
                    if (mDownSec.Start + mov < bMin) // too left
                        mov = bMin - mDownSec.Start;
                    else if (mDownSec.End + mov > bigM) // too right
                        mov = bigM - mDownSec.End;

                    Start = mDownSec.Start + mov;
                    End = mDownSec.End + mov;

                }
                else if (HeldComp == -1) // not the case with seekbar
                {
                    if (mov < 0)//  increasing length to left
                    {
                        if (mDownSec.Start + mov < bMin) // too left
                            mov = bMin - mDownSec.Start;
                    }
                    else
                    {
                        int nSToE = (int)Math.Round(((double)mDownSec.Start + mov) / (max - min) * Width);
                        int nEndToE = (int)Math.Round((double)End / (max - min) * Width);
                        if (nEndToE - nSToE < minZoom)
                        {
                            mov = Math.Round((nEndToE - minZoom) * ((double)max - min) / Width, 1) - mDownSec.Start;
                        }
                    }
                    Start = mDownSec.Start + mov;
                }
                else if (HeldComp == 1)
                {
                    if (mov > 0)//  increasing length to right
                    {
                        if (mDownSec.End + mov > bigM) // too right
                            mov = bigM - mDownSec.End;
                    }
                    else
                    {
                        int nSToE = (int)Math.Round((double)Start / (max - min) * Width);
                        int nEndToE = (int)Math.Round((double)(mDownSec.End + mov) / (max - min) * Width);

                        if (nEndToE - nSToE < minZoom)
                        {
                            mov = Math.Round(minZoom * ((double)max - min) / Width, 1) + Start - mDownSec.End;
                        }
                    }
                    End = mDownSec.End + mov;
                }
            }

			method();
			curLoc = e;
			return c;
		}
	}
    public class LayerClip : Clip
    {
        protected LayerClip(double start, double end, string source = "") : base(start, end, source)
        {

        }
        public override async Task OnPaintBeforeAsync(int layerIndex, int layersCount, double min, double max, int Width, int Height, Graphics g, double bMin, double bMax)
        {
            //this rect will be used for overview section
            Rectangle zsRec2 = new Rectangle();
            try
            {
                //calculate the rectangle.
                // for zoom section, min/max is got from iterating all other sections. 
                // End of first section on the left is min. if there is no section, min of Start of zoom Bar is min
                // Start of first section on the right is min. if there is no section, max of Start of zoom Bar is max
                // Width is not same as max. Width is the graphical length.
                // Stays the same for all layers
                float layerHeight = (Height - ZoomBarHeight * 2) / (float)layersCount;
                zsRec = new RectangleF(
                    (int)Math.Round(((double)Start - min) / (max - min) * Width),
                    ZoomBarHeight * 2 + layerHeight * layerIndex,
                    (int)Math.Round(((double)End - Start) / (max - min) * Width),
                    layerHeight - 1);
                //
                zsRec2 = new Rectangle(
                    (int)Math.Round(((double)Start - bMin) / (bMax - bMin) * Width),
                    ZoomBarHeight,
                    (int)Math.Round(((double)End - Start) / (bMax - bMin) * Width),
                    ZoomBarHeight);

                Color cNormal = Color.FromArgb(100, this.Color);
                Color cHover = Color.FromArgb(160, this.Color);
                Color cSelect = Color.FromArgb(250, this.Color);
                Color cHeld = Color.FromArgb(230, this.Color);


                Color c1 = cNormal;
                Color c2 = c1;
                Color c3 = c1;

                //hover colors
                if (hoverOver == 1)
                    c3 = cHover;
                else if (hoverOver == -1)
                    c1 = cHover;
                else if (hoverOver == 0)
                {
                    c1 = cHover;
                    c2 = cHover;
                    c3 = cHover;
                }


                //held Colors
                if (HeldComp == 0 && !selected)
                {
                    c1 = cHeld;
                    c2 = c1;
                    c3 = c1;
                }
                else if (HeldComp == -1)
                    c1 = cHeld;
                else if (HeldComp == 1)
                    c3 = cHeld;

                //select Colors
                if (selected)
                {
                    c2 = cSelect;
                    if (HeldComp < -1 || HeldComp > 1)
                    {
                        c1 = cSelect;
                        c3 = c1;
                    }

                }
                float frac = (float)Math.Max(30 / zsRec.Width, 0.02);
                var positions = new[] { 0, frac, 1F - frac, 1f };
                var colors = new Color[] { c1, c2, c2, c3 };
                g.FillRectangle(colors, positions, zsRec.X, zsRec.Y, zsRec.Width, zsRec.Height);
                //
                g.DrawRectangle(
                    c2, 1,
                    zsRec2.X, zsRec2.Y, Math.Max(zsRec2.Width, 1), ZoomBarHeight);
                g.FillRectangle(selected ? cSelect : cNormal, zsRec2.X, zsRec2.Y, zsRec2.Width, ZoomBarHeight);

            }
            catch { }
            //g.FillRectangle((Brush)new SolidBrush(Color.FromArgb(30, 0, 255, 0)), zsRec);
        }

        public override Cursor MouseMove(int layerIndex, int layersCount, Point e, double max, double min, int Width, int Height, Cursor c, InvalidateRef method, double bMin, double bigM)
        {
            int eToSs = (int)Math.Round(e.X * (max - min) / Width + min);
            int sToE = (int)Math.Round(((double)Start - min) / (max - min) * Width);
            int eToE = (int)Math.Round(((double)End - min) / (max - min) * Width);

            //determine where we are hovering
            // 0 is center hover, -1, left edge hover, +1 is right edge hover, -2 is left section out, 2 is right section out, 3 is out of region
            // seek bar cannot be -1 or 1
            if (HeldComp < -1 || HeldComp > 1)
            {
                int inTol = 4, outTol = 4;

                // determine a hover section. dont consider y position yet
                if (e.X >= sToE - outTol && e.X <= sToE + inTol)
                {
                    c = Cursors.VSplit; hoverOver = -1;
                }
                else if (e.X >= eToE - inTol && e.X <= eToE + outTol)
                { c = Cursors.VSplit; hoverOver = 1; }
                else if (e.X >= sToE && e.X <= eToE)
                { c = Cursors.NoMoveHoriz; hoverOver = 0; }
                else
                {
                    if (eToSs < Start)
                        hoverOver = -2;
                    else
                        hoverOver = 2;
                    c = Cursors.Default;
                }
                // y position correction
                float layerHeight = (Height - ZoomBarHeight * 2 - sbh) / (float)layersCount;
                if (e.Y <= ZoomBarHeight * 2 + layerHeight * layerIndex ||
                    e.Y >= ZoomBarHeight * 2 + layerHeight * (layerIndex + 1))
                {
                    c = Cursors.Default;
                    hoverOver = 3;
                }
            }

            // already held
            if (HeldComp >= -1 && HeldComp <= 1)
            {
                // for length changing bars

                double minT = 1 / 30.0; // 30fps
                int movE = e.X - mDownOn.X;

                double mov = Math.Round(movE * ((double)max - min) / Width, 2);
                //seek bar cannot be -1 or 1, its 0, -2, 2, or 3
                if (HeldComp == 0) // draging
                {
                    if (mDownSec.Start + mov < bMin) // too left
                        mov = bMin - mDownSec.Start;
                    else if (mDownSec.End + mov > bigM) // too right
                        mov = bigM - mDownSec.End;

                    Start = mDownSec.Start + mov;
                    End = mDownSec.End + mov;
                }
                else if (HeldComp == -1) // not the case with seekbar
                {
                    if (mov < 0)//  increasing length to left
                    {
                        if (mDownSec.Start + mov < bMin) // too left
                            mov = bMin - mDownSec.Start;
                    }
                    else
                    {
                        if (mDownSec.End - mDownSec.Start - mov < minT) // less than X sec
                            mov = mDownSec.End - minT - mDownSec.Start;
                        DEBUG((mDownSec.End - mDownSec.Start - mov).ToString());
                    }
                    Start = mDownSec.Start + mov;
                }
                else if (HeldComp == 1)
                {
                    if (mov > 0)//  increasing length to right
                    {
                        if (mDownSec.End + mov > bigM) // too right
                            mov = bigM - mDownSec.End;
                    }
                    else
                    {
                        if (mDownSec.End - mDownSec.Start + mov < minT) // less than X sec
                            mov = -mDownSec.End + minT + mDownSec.Start;
                    }
                    End = mDownSec.End + mov;
                }

            }

            method();
            curLoc = e;
            return c;
        }
    }
	public class AudioClip : LayerClip
    {
		private double[] left;
		private double[] right;

		public byte[] Data { get; private set; }// convert two bytes to one double in the range -1 to 1
        public byte[] WaveData { get; }

        static double bytesToDouble(byte firstByte, byte secondByte)
		{
			// convert two bytes to one short (little endian)
			short s = (short)((secondByte << 8) | firstByte);
			// convert to range from -1 to (just below) 1
			return s / 32768.0;
		}
		// Returns left and right double arrays. 'right' will be null if sound is mono.
		public void openWav(out double[] left, out double[] right)
		{
            var wav = WaveData;
            left = null;
            right = null;
            if (Data == null)
                return;
			// Determine if mono or stereo
			int channels = wav[22];     // Forget byte 23 as 99.999% of WAVs are 1 or 2 channels

			// Get past all the other sub chunks to get to the data subchunk:
			int pos = 12;   // First Subchunk ID from 12 to 16

			// Keep iterating until we find the data chunk (i.e. 64 61 74 61 ...... (i.e. 100 97 116 97 in decimal))
			while (!(wav[pos] == 100 && wav[pos + 1] == 97 && wav[pos + 2] == 116 && wav[pos + 3] == 97))
			{
				pos += 4;
				int chunkSize = wav[pos] + wav[pos + 1] * 256 + wav[pos + 2] * 65536 + wav[pos + 3] * 16777216;
				pos += 4 + chunkSize;
			}
			pos += 8;

			// Pos is now positioned to start of actual sound data.
			int samples = (wav.Length - pos) / 2;     // 2 bytes per sample (16 bit sound mono)
			if (channels == 2) samples /= 2;        // 4 bytes per sample (16 bit stereo)

			// Allocate memory (right will be null if only mono sound)
			left = new double[samples];
			if (channels == 2) right = new double[samples];
			else right = null;

			// Write to double array/s:
			int i = 0;
			while (pos < Data.Length)
			{
				left[i] = Math.Log(bytesToDouble(wav[pos], wav[pos + 1]));
				pos += 2;
				if (channels == 2)
				{
					right[i] = Math.Log(bytesToDouble(wav[pos], wav[pos + 1]));
					pos += 2;
				}
				i++;
			}
		}
		public AudioClip(double start, double end, byte[] data, string fname, byte[] waveData) : base(start, end, fname)
        {
            Data = data;
            WaveData = waveData;
            //openWav(out double[] left, out double[] right);
            this.left = left;
			this.right = right;
			this.Color = System.Drawing.Color.FromArgb(30, 168, 150);
		}
        protected float getLeft(double t)
        {
            var ind = (int)(t * 16000);
            if (ind >= left.Length)
                return 0;
            if (ind <= 0)
                return 0;
            else return (float)left[ind];
        }
		public override async Task OnPaintBeforeAsync(int layerIndex, int layersCount, double min, double max, int Width, int Height, Graphics g, double bMin, double bMax)
		{
			await base.OnPaintBeforeAsync(layerIndex, layersCount, min, max, Width, Height, g, bMin, bMax);
            if (Data == null)
                return;
            float layerHeight = (Height - ZoomBarHeight * 2 - sbh) / (float)layersCount;
			var secRec = new RectangleF(
				(int)Math.Round(((double)Start - min) / (max - min) * Width),
				ZoomBarHeight * 2 + layerHeight * layerIndex,
				(int)Math.Round(((double)End - Start) / (max - min) * Width),
				layerHeight - 1);
            // get time per pixle
            float tpp = (float)(1 * (max - min) / Width);

            var levels = new List<SKPoint>();
            var tMin = Start;
            var tMax = End;
            if (tMin < min)
                tMin = min;
            if (tMax > max)
                tMax = max;
            if (left == null)
                return;
			for (double t = 0; t <= End - Start; t += tpp)
            {
                var level = Math.Abs(getLeft(t));
                var xPos = (float)((t) / tpp + (Start - min) / tpp);
                levels.Add(new SKPoint(xPos, level));
			}
            var maxLevel = levels.Max(l => l.Y);
            var minLevel = levels.Min(l => l.Y);
            for (int i = 0; i < levels.Count; i++)
            {
                var layerStartY = ZoomBarHeight * 2 + layerHeight * layerIndex;
                // Normalize Y
                levels[i] = new SKPoint(levels[i].X, (levels[i].Y - minLevel) / (maxLevel - minLevel)  * layerHeight);
                // Find yPos
                float h = levels[i].Y - 1;
                if (h < 1)
                    h = 1;
                float yMin = layerStartY + (layerHeight - h) / 2;
                float yMax = yMin + h;
                g.DrawLine(Color.White, 1, levels[i].X, yMin, levels[i].X, yMax);
            }
            
		}
	}
    public class VideoClip : LayerClip
	{
		public VideoClip(double start, double end) : base(start, end, "")
		{
			this.Color = System.Drawing.Color.FromArgb(255, 113, 91);
		}
		public CachedSKImage[]? Data { get; set; }
		public float Size { get; set; } = 100;
        public SKBlendMode BlendMode { get; set; } = SKBlendMode.SrcOver;
        public VideoClipEditor.StretchingModes StretchingMode { get; set; } = VideoClipEditor.StretchingModes.Clip;
        public int Opacity { get; set; } = 255;
        public float X { get; set; } = 50;
		public float Y { get; set; } = 50 * 9 / 16.0F;
		public float fps { get; set; } = 30;
        int getIndex(double position)
        {
            if (!(Data != null && position >= Start && position <= End))
                return -1;
            if (Data.Length == 0)
                return -1;
            if (StretchingMode == VideoClipEditor.StretchingModes.Clip)
            {
                var relPos = position - Start;
                var fps = 30;
                int index = (int)(relPos * fps);
                if (index >= 0 && index < Data.Length)
                    return index;
                return -1;
            }
            else if (StretchingMode == VideoClipEditor.StretchingModes.Stretch)
            {
                var fractionTimeToRender = (position - this.Start) / (End - Start);
                var index = (int)(Math.Round((Data.Length - 1) * fractionTimeToRender));
                if (index >= 0 && index < Data.Length)
                    return index;
                return -1;
            }
            else // if (StretchingMode == VideoClipEditor.StretchingModes.Loop)
            {
                var relPos = position - Start;
                var fps = 30;
                int index = (int)(relPos * fps);
                if (index < 0) 
                    return -1;
                while (index >= Data.Length)
                    index -= Data.Length;
                return index;
            }
        }
		public override async Task RenderAsync(double position, SKCanvas canvas, RenderConfig config)
		{
            if (Data != null && position >= Start && position <= End)
            {
                var indexToRender = getIndex(position);
                if (indexToRender < 0) // cant render
                    return;
                var bmp = await Data[indexToRender].Get();
                if (bmp == null)
                {
                    //Console.WriteLine("bmp null at: " + indexToRender);
                    //await (RenderAsync(position, canvas, config));
                    return;
                }
                float aspect = bmp.Width / (float)bmp.Height;

                var wid = /* We are gonna force fit */ 100 * (/* User Scale */Size / 100);
                var hei = wid / aspect;
                var x = X - wid / 2;
                var y = Y - hei / 2;
                RectangleF r = new RectangleF(x, y, wid, hei);
                // Create an SKPaint with blend mode
                using (SKPaint paint = new SKPaint
                {
                    BlendMode = this.BlendMode, // or any other Dodge blend mode you prefer
                    IsAntialias = true // enable antialiasing if needed
                })
                {
                    //Console.WriteLine("Render VideoClip: " + Source+" " + Label + ": " + indexToRender);
                    paint.Color = paint.Color.WithAlpha((byte)Opacity);
                    canvas.DrawImage(bmp, new SKRect(r.Left, r.Top, r.Right, r.Bottom), paint);
                }
			}
            else
            { }
		}
        public async Task CacheFrame(double position)
        {
            // No need for this on the server
            //if (Data != null && position >= Start && position <= End)
            //{
            //    var indexToRender = getIndex(position);
            //    if (indexToRender >= 0)
            //        await Data[indexToRender].GetSKBimap();
            //}
            //else
            //{ }
        }
        public void FreeFrameCache(double position)
        {
            if (Data != null && position >= Start && position <= End)
            {
                var indexToRender = getIndex(position);
                if (indexToRender >= 0)
                    Data[indexToRender].FreeALL();
            }
            else
            { }
        }
        public override async Task OnPaintBeforeAsync(int layerIndex, int layersCount, double min, double max, int Width, int Height, Graphics g, double bMin, double bMax)
		{
			await base.OnPaintBeforeAsync(layerIndex, layersCount, min, max, Width, Height, g, bMin, bMax);
			// Draw overlay image
			if (Data != null)
			{
				//// determine time per length unit equal to width of frame
				//float layerHeight = (Height - ZoomBarHeight * 2 - sbh) / (float)layersCount;
				//var frameWidth = (layerHeight - 3) * (await Data[0].Get()).Width / (await Data[0].Get()).Height;
    //            float gToV (float xg) => (float)(xg * (max - min) / Width + min);
    //            float vToG(float xv) => (float)((xv - min) / (max - min) * Width);
    //            var v1 = gToV(0);
				//var v2 = gToV(frameWidth);
    //            var dv = v2 - v1;
				//for (var xv = Start; xv < End; xv+= dv) // starting from minv, increment xv in frame width equivalent intervals				
    //            {
    //                // we have the v now. get the frame;
    //                var index = getIndex(xv);
    //                if (index < 0)
				//		continue;
    //                var data = await Data[index].Get();

				//	var zsRec = new RectangleF(
    //                    (int)Math.Round(((double)xv - min) / (max - min) * Width) + 1,
    //                    ZoomBarHeight * 2 + layerHeight * layerIndex + 1,
    //                    frameWidth,
    //                    layerHeight - 3);
    //                g.canvas.DrawImage(data, new SKRect(zsRec.Left, zsRec.Top, zsRec.Right, zsRec.Bottom));
    //            }
			}
		}
    }
    public class PhotoClip : LayerClip
    {
        public SKImage Data { get; set; }
        public float Size { get; set; } = 100;
        public SKBlendMode BlendMode { get; set; } = SKBlendMode.SrcOver;
        public int Opacity { get; set; } = 255;
        public float X { get; set; } = 50;
        public float Y { get; set; } = 50 * 9 / 16.0F;
        public PhotoClip(double start, double end) : base(start, end, "")
        {
			this.Color = System.Drawing.Color.FromArgb(120, 0, 0);
        }
        public override async Task RenderAsync(double position, SKCanvas canvas, RenderConfig config)
        {
            if (Data != null && position >= Start && position <= End)
            {
                float aspect = Data.Width / (float)Data.Height;
                var wid = /* We are gonna force fit */ 100 * (/* User Scale */Size / 100);
                var hei = wid / aspect;
                var x = X - wid / 2;
                var y = Y - hei / 2;
                RectangleF r = new RectangleF(x, y, wid, hei);
                // Create an SKPaint with blend mode
                using (SKPaint paint = new SKPaint
                {
                    BlendMode = this.BlendMode, // or any other Dodge blend mode you prefer
                    IsAntialias = true // enable antialiasing if needed
                })
                {
                    //Console.WriteLine("Render Photo: " + Source + " " + Label);
                    paint.Color = paint.Color.WithAlpha((byte)Opacity);
                    canvas.DrawImage(Data, new SKRect(r.Left, r.Top, r.Right, r.Bottom), paint);
                }
            }
        }
		public override async Task OnPaintBeforeAsync(int layerIndex, int layersCount, double min, double max, int Width, int Height, Graphics g, double bMin, double bMax)
		{
			await  base.OnPaintBeforeAsync(layerIndex, layersCount, min, max, Width, Height, g, bMin, bMax);
            // Draw overlay image
            if (Data != null)
            {
				float layerHeight = (Height - ZoomBarHeight * 2) / (float)layersCount;
				var zsRec = new RectangleF(
					(int)Math.Round(((double)Start - min) / (max - min) * Width) + 1,
					ZoomBarHeight * 2 + layerHeight * layerIndex + 1,
					(layerHeight - 3) * Data.Width / Data.Height,
					layerHeight - 3);
                g.canvas.DrawImage(Data, new SKRect(zsRec.Left, zsRec.Top, zsRec.Right, zsRec.Bottom));
            }
		}
	}
    public class SubtitleClip : LayerClip
    {
        public SubtitleClip(double start, double end, string source) : base(start, end, source)
        {
			this.Color = System.Drawing.Color.FromArgb(255, 190, 11);
        }
        public SKBlendMode BlendMode { get; set; } = SKBlendMode.SrcOver;
        public int Opacity { get; set; } = 255;
        void DrawWrapLines(float x, float y, string longLine, float lineLengthLimit, SKCanvas canvas, SKPaint defPaint, RenderConfig config)
        {
            var wrappedStrings = new List<string>();
            var lineLength = 0f;
            var line = "";
            foreach (var word in longLine.Split(' ', '\n'))
            {
                var wordWithSpace = word + " ";
                var wordWithSpaceLength = defPaint.MeasureText(wordWithSpace);
                if (lineLength + wordWithSpaceLength > lineLengthLimit)
                {
                    wrappedStrings.Add(line.Trim());
                    line = "" + wordWithSpace;
                    lineLength = wordWithSpaceLength;
                }
                else
                {
                    line += wordWithSpace;
                    lineLength += wordWithSpaceLength;
                }
            }
            if (line.Length > 0)
                wrappedStrings.Add(line.Trim());

            // Now lets calculate the bounding rect of this line.

            var wrappedLines = new Dictionary<SKRect, string>();
            float dy = 0;
            foreach (var wrappedLine in wrappedStrings)
            {
                var skrect = new SKRect();
                defPaint.MeasureText(wrappedLine, ref skrect);
                skrect.Top += dy;
                skrect.Bottom += dy;
                dy += defPaint.FontSpacing;
                wrappedLines.Add(skrect, wrappedLine);
            }

            var totalHeight = defPaint.FontSpacing * wrappedStrings.Count;


            // Find the bounding rect Ys.
            var top = wrappedLines.Min(r => r.Key.Top);
            var bottom = wrappedLines.Max(r => r.Key.Bottom);
            var middle = (bottom + top) / 2;

            // The original rects want to get drawn from top to bottom.
            // To force the middle to go to 0, we need to add an offset
            var yOffset = - middle;
            // If we draw with this offset, bounding rect will be placed at 0
            // to force it to go to y, we just need to add y
            yOffset += y;
            // Now, we just need to draw at y = yoffset + d(line spacing) * i

            // lets draw now

            dy = 0;

            foreach (var wrappedLine in wrappedLines)
            {
                // Lets now calculate xOffset for centered placement at x
                
                var rc = wrappedLine.Key;
                if (config.ShadowSize > 0)
                    defPaint.ImageFilter = SKImageFilter.CreateDropShadow(config.ShadowDistance, config.ShadowDistance, config.ShadowSize, config.ShadowSize, config.ShadowColor);
                canvas.DrawText(wrappedLine.Value, x - rc.Width / 2, yOffset + dy, defPaint);
                yOffset += defPaint.FontSpacing;
            }
        }
        public override async Task RenderAsync(double position, SKCanvas canvas, RenderConfig config)
        {
            if (Source?.Trim().Length > 0 && position >= Start && position <= End)
            {
                float opacity = 1;
                if (position <= Start + config.SubtitleOverlap)
                    opacity = 1 - (float)((Start + config.SubtitleOverlap - position) / config.SubtitleOverlap);
                else if (position > End - config.SubtitleOverlap)
                    opacity = 1 - (float)((position - (End - config.SubtitleOverlap)) / config.SubtitleOverlap);
                DrawWrapLines(config.SubtitleLocation.X, config.SubtitleLocation.Y, Source, 90, canvas,
                new SKPaint(config.SubTitlesFont)
                {
                    Color = config.SubtitleColor.WithAlpha((byte)(Opacity /*0-255*/ * opacity /*0-1*/)),
                    IsAntialias = true,
                    BlendMode = this.BlendMode,
                }, config);
            }
        }
        //public class PNGFrame
        //{
        //    SKBitmap data;
        //    public SKBitmap Image
        //    {
        //        get
        //        {
        //            if (data == null)
        //                try
        //                {
        //                    SKBitmap.Decode()
        //                }
        //                catch { }
        //            return data;
        //        }
        //    }
        //    string file = "";
        //    byte[] fileData;
        //    public PNGFrame(FFmpegBlazor.FFMPEG ffmpeg, byte[] dataBuffer)
        //    {
        //        this.file = fileName;
        //        dataBuffer =
        //    }
        //}
    }
    public class RenderConfig
    {
        public SKSize TargetSize { get; set; }
        public float AspectRatio { get; set; }
        public float ShadowSize { get; set; }
        public float ShadowDistance { get; set; }
        public SKFont SubTitlesFont { get; set; }
        public SKColor SubtitleColor { get; set; }
		public SKColor ShadowColor { get; set; }

        public float SubtitleOverlap { get; set; } = 0.2F;
        public SKPoint SubtitleLocation { get; set; }
    }
   // public class HybridSKBitmap 
   // {
   //     public string? FFMpegFile { get; set; }
   //     SKBitmap Bitmap;
   //     //public FFMPEG? FFMpeg { get; set; }
   //     //public HybridSKBitmap(string fName, FFMPEG fFMPEG, SKBitmap data = null)
   //     //{
   //     //    FFMpegFile = fName;
   //     //    FFMpeg = fFMPEG;
   //     //}
   //     public void FreeALL()
   //     {
   //         //if (FFMpegFile != "")
   //         //{
   //         //    FFMpegFile = ""; // mark that we dont have a file at the back
   //         //    FFMpeg?.UnlinkFile(FFMpegFile);
   //         //}
   //         if (Bitmap != null)
   //         {
   //             Bitmap.Dispose();
   //             Bitmap = null;
   //         }
			//GC.Collect();
   //     }
   //     public async Task<SKBitmap> GetSKBimap()
   //     {
   //         if (Bitmap == null)
   //         {
   //             try
   //             {
   //                 ////Console.WriteLine("Get SKBitmap for: " + FFMpegFile);
   //                 //var buffer = await FFMpeg?.ReadFile(FFMpegFile);
   //                 //Bitmap = SKBitmap.Decode(buffer);
   //             }
   //             catch (Exception ex)
   //             {
   //                 Console.WriteLine("Exception while decoding: " + ex);
   //             }
   //         }
   //         return Bitmap;
   //     }
   // }
}