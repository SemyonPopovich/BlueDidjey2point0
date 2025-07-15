class BotHead {
    static void Main(string[] args) {
        MainAsync().GetAwaiter().GetResult();
    }
    static async Task MainAsync() {
        var discord = new DiscordClient(new DiscordConfiguration {
            Token = "MTM0MzU4NDkzMjA0NzQ5MTEwMg.GWtbCu.1gJmXTm9Ihxj2b_Lz2TE9I5CtJ45-9uxt3bLjs",
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All
        });
        discord.UseVoiceNext(new VoiceNextConfiguration {
            EnableIncoming = false
        });
        var commands = discord.UseCommandsNext(new CommandsNextConfiguration {
            StringPrefixes = new[] { "!" }
        });
        commands.RegisterCommands<BotCommands>();

        await discord.ConnectAsync();
        await Task.Delay(-1);

        discord.UseInteractivity(new InteractivityConfiguration {
            Timeout = TimeSpan.FromMinutes(1)
        });
    }
}
public class Constant {
    public static string YoutubeDLPath = "yt-dlp.exe";
    public static string FFmpegPath = "../ffmpeg-2025-02-24-git-6232f416b1-essentials_build/bin/ffmpeg.exe";
}