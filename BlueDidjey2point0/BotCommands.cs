using System.Reflection.Metadata;
using YoutubeDLSharp.Options;

public class BotCommands : BaseCommandModule {
    Queue<string> songsQueue = new Queue<string>();
    public List<string> songsNameslist = new List<string>();
    private DiscordChannel textChannel;
    private CancellationTokenSource currentSongToken = new CancellationTokenSource();
    private VoiceNextConnection currentChannel;
    private FileStream currentSongStream;
    //private static readonly OptionSet DefaultOptions = new OptionSet {
    //    AudioFormat = AudioConversionFormat.Wav,
    //    PostprocessorArgs = new[] {
    //        "ffmpeg:-ar 48000 -ac 2 -af atempo=0.95"
    //    }
    //};
    //,progress: null, output: null, overrideOptions: DefaultOptions
    private readonly YoutubeDLSharp.YoutubeDL ytdl = new YoutubeDLSharp.YoutubeDL {
        YoutubeDLPath = Constant.YoutubeDLPath,
        FFmpegPath = Constant.FFmpegPath,
    };
    

    [Command("play")]
    public async Task play(CommandContext ctx, string url) {
        if (isUserInVoiceChat(ctx)) {
            textChannel = ctx.Channel;
            await joinChannel(ctx);
            await addQueue(url);
            if(songsQueue.Count == 1) _ = playQueue();
        } else {
            await ctx.RespondAsync("Зайди в канал");
        }
    }
    
    [Command("skip")]
    public async Task skip(CommandContext ctx) {
        await currentChannel.SendSpeakingAsync(false);
        currentSongToken.Cancel();
        await ctx.RespondAsync("Песня пропущена");
    }

    [Command("chekq")]
    public async Task ShowQueue(CommandContext ctx) {
        if (songsQueue.Count == 0) {
            await ctx.RespondAsync("Очередь пуста.");
        } else {
            var numbers = songsNameslist.Select((title, i) => $"{i + 1}. {title}");
            await ctx.RespondAsync("Очередь песен:\n" + string.Join("\n", numbers));
        }
    }

    [Command("clearq")]

    public async Task ClearQuquq(CommandContext ctx) {
        if (songsQueue.Count > 1) {
            var currentUrl = songsQueue.Peek();
            var currentName = songsNameslist[0];
            songsQueue.Clear();
            songsQueue.Enqueue(currentUrl);
            songsNameslist.Clear();
            songsNameslist.Add(currentName);
            await ctx.RespondAsync("Отчистил очередь");
        }     
    }

    [Command("leave")]

    public async Task leave(CommandContext ctx) {
        currentSongToken?.Cancel();
        songsQueue.Clear();
        songsNameslist.Clear();
        await currentChannel.SendSpeakingAsync(false);
        currentChannel.Disconnect();
        await ctx.RespondAsync("Пока");
    }

    private async Task playQueue() {
        currentSongToken = new CancellationTokenSource();
        if (songsNameslist.Count > 0) { try { await textChannel.SendMessageAsync($"▶ Сейчас играет: {songsNameslist[0]}");} catch { } }                  
        try { await playUrl(takeSongFromQueue()); } catch (OperationCanceledException) { } finally { songsQueue.Dequeue(); songsNameslist.RemoveAt(0); }       
        if (songsQueue.Count > 0) {
            await playQueue();
        }
    }
    private async Task playUrl(string url) {
        var result = await ytdl.RunAudioDownload(url,  AudioConversionFormat.Wav);

        if (result.Success)
            await playAndDelete(result.Data);
    }
    private async Task playWav(string path) {
        await currentChannel.SendSpeakingAsync(true);
        await using (currentSongStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Delete)) {
            await currentSongStream.CopyToAsync(currentChannel.GetTransmitSink(), cancellationToken: currentSongToken.Token);
        }
    }

    private async Task playAndDelete(string path) {
        try { await playWav(path); } finally { File.Delete(path); }
    } 

    private bool isUserInVoiceChat(CommandContext ctx) {
        var voiceChannel = ctx.Member.VoiceState?.Channel;
        return voiceChannel != null;
    }

    private async Task joinChannel(CommandContext ctx) {
        if (currentChannel == null) {
            var VoiceNext = ctx.Client.GetVoiceNext();
            var voiceChannel = ctx.Member?.VoiceState?.Channel;
            currentChannel = await voiceChannel.ConnectAsync();
        } else { }
    }

    private async Task addQueue(string url) { 
        songsQueue.Enqueue(url);
        var info = await ytdl.RunVideoDataFetch(url);
        var songInfo = info.Data.Title;       
        songsNameslist.Add(songInfo);
    }

    private string takeSongFromQueue() {
        var nextSong = songsQueue.Peek();
        return nextSong;
    }
}

