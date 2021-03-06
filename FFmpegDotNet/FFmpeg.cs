﻿using System;
using System.IO;
using System.Text.RegularExpressions;

namespace FFmpegDotNet
{
	public class FFmpeg
	{
        public static string Main { get; set; } = Path.Combine("plugin", $"ffmpeg32", "ffmpeg");
		public static string Probe { get; set; } = Path.Combine("plugin", $"ffmpeg32", "ffprobe");

        public class Stream : Get
		{
			public Stream(string filePath) : base(filePath)
			{

			}
		}

		public int FrameCount(string filePath, int mapId)
		{
			var file = Path.Combine(Path.GetTempPath(), $"nemu_{new Random().Next(0, 999999999):D9}");
			new Run().Execute($"\"{Main}\" -hide_banner -i \"{filePath}\" -map 0:{mapId} -vcodec copy -an -sn -dn -f null - 2> {file}", Path.GetTempPath());

			string text = File.ReadAllText(file);
			var match = Regex.Matches(text, @"(\d+) fps=", RegexOptions.Multiline);

			int frames = 0;
			int.TryParse(match[match.Count - 1].Groups[1].Value, out frames);

			if (File.Exists(file))
				File.Delete(file);

			return frames;
        }

		public int FrameCountAccurate(string filePath, int trackId)
		{
			var file = Path.Combine(Path.GetTempPath(), $"nemu_{new Random().Next(0, 999999999):D9}");
			new Run().Equals($"\"{Probe}\" -threads {Environment.ProcessorCount * 2} -v quiet -pretty -print_format csv -select_streams v:{trackId} -count_frames -show_entries \"stream=nb_read_frames\" {filePath} > {file}");

			string text = File.ReadAllText(file);
			var match = Regex.Match(text, @"stream,(\d+)");

			int frames = 0;
			int.TryParse(match.Groups[1].Value, out frames);

			if (File.Exists(file))
				File.Delete(file);

			return frames;
        }
	}
}