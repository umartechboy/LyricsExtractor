using FFMpegCore;
using Microsoft.AspNetCore.SignalR;
using System.Globalization;
using System.Reflection;
using Whisper;

namespace LyricsExtractor.Services
{
    public class Common
	{
		public static FFOptions binInfo 
        {
            get => new FFOptions()
            {
                BinaryFolder = "FFMpeg/bin",
                TemporaryFilesFolder = "FFMpeg/temp"
            };
        }
        public static string serializeSSegment(sSegment seg)
        {
            var line = 
                seg.time.begin.ToString() + 
                ">>" +
				seg.time.end.ToString() +
                ">>" +
                seg.text;

            return line;
        }
		public static void deserializeSSegment(string segStr, out TimeSpan begin, out TimeSpan end, out string text)
		{
            var parts = segStr.Split(">>");
            begin = TimeSpan.Parse(parts[0]);
			end = TimeSpan.Parse(parts[1]);
			text = parts[2];
		}
		public static string shortCheckSum(byte[] data)
		{
			var cSum = new byte[16];
			byte sum = 0;
			for (int i = 0; i < data.Length; i++)
			{
				sum ^= data[i];
				cSum[i % cSum.Length] ^= sum;
			}
			return Convert.ToBase64String(cSum).Replace("\\","-").Replace("/","-");
		}
	}
/// <summary>Implementation of Callbacks abstract class, to print these segments as soon as they’re produced by the library.</summary>
sealed class WhisperReader : Callbacks
    {
        public delegate void WhisperSegmentHandler(sSegment sm, float probability);
        public event WhisperSegmentHandler OnSegment;
        protected override bool onEncoderBegin(Context sender)
        {
            return base.onEncoderBegin(sender);
        }
        protected override void onNewSegment(Context sender, int countNew)
        {
            TranscribeResult res = sender.results(eResultFlags.Timestamps);
            ReadOnlySpan<sToken> tokens = res.tokens;

            int s0 = res.segments.Length - countNew;
            if (s0 == 0)
                Console.WriteLine();

            for (int i = s0; i < res.segments.Length; i++)
            {
                float p = 0;
                int count = 0;
                //foreach (sToken tok in res.getTokens(res.segments[i]))
                //{
                //    p += tok.probability;
                //    count++;
                //}
                //p /= count;
                OnSegment?.Invoke(res.segments[i], 0);
            }
        }
    }

    sealed record class CommandLineArgs
    {
        public int n_threads = Environment.ProcessorCount;
        public int offset_t_ms = 0;
        public int offset_n = 0;
        public int duration_ms = 0;
        public int max_context = -1;
        public int max_len = 0;

        public float word_thold = 0.01f;

        public bool speed_up = false;
        public bool translate = false;
        public bool diarize = false;
        public bool output_txt = false;
        public bool output_vtt = false;
        public bool output_srt = false;
        public bool print_special = false;
        public bool print_progress = false;
        public bool print_colors = true;
        public bool no_timestamps = false;
        public string? prompt = null;

        public eLanguage language = eLanguage.English;
        public string model = string.Empty;
        public readonly List<string> fileNames = new List<string>();

        const bool output_wts = false;
        public void apply(ref Parameters p)
        {
            p.setFlag(eFullParamsFlags.PrintRealtime, false);
            p.setFlag(eFullParamsFlags.PrintProgress, print_progress);
            p.setFlag(eFullParamsFlags.PrintTimestamps, !no_timestamps);
            p.setFlag(eFullParamsFlags.PrintSpecial, print_special);
            p.setFlag(eFullParamsFlags.Translate, translate);
            p.language = language;
            p.cpuThreads = n_threads;
            if (max_context >= 0)
                p.n_max_text_ctx = max_context;
            p.offset_ms = offset_t_ms;
            p.duration_ms = duration_ms;
            p.setFlag(eFullParamsFlags.TokenTimestamps, output_wts || max_len > 0);
            p.thold_pt = word_thold;
            p.max_len = output_wts && max_len == 0 ? 60 : max_len;
            p.setFlag(eFullParamsFlags.SpeedupAudio, speed_up);
        }

        public eResultFlags resultFlags()
        {
            eResultFlags flags = eResultFlags.None;
            bool wts = output_wts || max_len > 0;
            if (!no_timestamps || wts)
                flags |= eResultFlags.Timestamps;
            if (wts || print_colors)
                flags |= eResultFlags.Tokens;
            return flags;
        }

        static eLanguage parseLanguage(string lang) =>
            Library.languageFromCode(lang) ?? throw new ArgumentException($"Unknown language code \"{lang}\"");

        public CommandLineArgs():this(new string[0])
        { }
        public CommandLineArgs(string[] argv)
        {
            for (int i = 0; i < argv.Length; i++)
            {
                string arg = argv[i];
                if (arg[0] != '-')
                {
                    fileNames.Add(arg);
                    continue;
                }
                if (arg == "-h" || arg == "--help")
                {
                    printUsage();
                    throw new OperationCanceledException();
                }
                else if (arg == "-t" || arg == "--threads") n_threads = int.Parse(argv[++i]);
                else if (arg == "-ot" || arg == "--offset-t") offset_t_ms = int.Parse(argv[++i]);
                else if (arg == "-on" || arg == "--offset-n") offset_n = int.Parse(argv[++i]);
                else if (arg == "-d" || arg == "--duration") duration_ms = int.Parse(argv[++i]);
                else if (arg == "-mc" || arg == "--max-context") max_context = int.Parse(argv[++i]);
                else if (arg == "-ml" || arg == "--max-len") max_len = int.Parse(argv[++i]);
                else if (arg == "-wt" || arg == "--word-thold") word_thold = float.Parse(argv[++i], CultureInfo.InvariantCulture);
                else if (arg == "-su" || arg == "--speed-up") speed_up = true;
                else if (arg == "-tr" || arg == "--translate") translate = true;
                else if (arg == "-di" || arg == "--diarize") diarize = true;
                else if (arg == "-otxt" || arg == "--output-txt") output_txt = true;
                else if (arg == "-ovtt" || arg == "--output-vtt") output_vtt = true;
                else if (arg == "-osrt" || arg == "--output-srt") output_srt = true;
                else if (arg == "-ps" || arg == "--print-special") print_special = true;
                else if (arg == "-nc" || arg == "--no-colors") print_colors = false;
                else if (arg == "-pp" || arg == "--print-progress") print_progress = true;
                else if (arg == "-nt" || arg == "--no-timestamps") no_timestamps = true;
                else if (arg == "-l" || arg == "--language") language = parseLanguage(argv[++i]);
                else if (arg == "--prompt") prompt = argv[++i];
                else if (arg == "-m" || arg == "--model") model = argv[++i];
                else if (arg == "-f" || arg == "--file") fileNames.Add(argv[++i]);
                else
                    throw new ArgumentException($"Unknown argument: \"{arg}\"");
            }
        }

        static string cstr(bool b) => b.ToString();

        void printUsage()
        {
            Console.WriteLine();

            Console.WriteLine("usage: {0} [options] file0.mp3 file1.wma ...", Path.GetFileName(Assembly.GetExecutingAssembly().Location));
            Console.WriteLine();
            Console.WriteLine("options:");
            Console.WriteLine("  -h,       --help          [default] show this help message and exit");
            Console.WriteLine("  -t N,     --threads N     [{0,-7:D}] number of threads to use during computation", n_threads);
            Console.WriteLine("  -ot N,    --offset-t N    [{0,-7:D}] time offset in milliseconds", offset_t_ms);
            Console.WriteLine("  -on N,    --offset-n N    [{0,-7:D}] segment index offset", offset_n);
            Console.WriteLine("  -d  N,    --duration N    [{0,-7:D}] duration of audio to process in milliseconds", duration_ms);
            Console.WriteLine("  -mc N,    --max-context N [{0,-7:D}] maximum number of text context tokens to store", max_context);
            Console.WriteLine("  -ml N,    --max-len N     [{0,-7:D}] maximum segment length in characters", max_len);
            Console.WriteLine("  -wt N,    --word-thold N  [{0,-7:F2}] word timestamp probability threshold", word_thold);
            Console.WriteLine("  -su,      --speed-up      [{0,-7}] speed up audio by x2 (reduced accuracy)", cstr(speed_up));
            Console.WriteLine("  -tr,      --translate     [{0,-7}] translate from source language to english", cstr(translate));
            Console.WriteLine("  -di,      --diarize       [{0,-7}] stereo audio diarization", cstr(diarize));
            Console.WriteLine("  -otxt,    --output-txt    [{0,-7}] output result in a text file", cstr(output_txt));
            Console.WriteLine("  -ovtt,    --output-vtt    [{0,-7}] output result in a vtt file", cstr(output_vtt));
            Console.WriteLine("  -osrt,    --output-srt    [{0,-7}] output result in a srt file", cstr(output_srt));
            Console.WriteLine("  -ps,      --print-special [{0,-7}] print special tokens", cstr(print_special));
            Console.WriteLine("  -nc,      --no-colors     [{0,-7}] do not print colors", cstr(!print_colors));
            Console.WriteLine("  -nt,      --no-timestamps [{0,-7}] do not print timestamps", cstr(no_timestamps));
            Console.WriteLine("  -l LANG,  --language LANG [{0,-7}] spoken language", language.getCode());
            Console.WriteLine("            --prompt PROMPT [       ] initial prompt");
            Console.WriteLine("  -m FNAME, --model FNAME   [{0,-7}] model path", model);
            Console.WriteLine("  -f FNAME, --file FNAME    [{0,-7}] path of the input audio file", "");
        }
    }
}
