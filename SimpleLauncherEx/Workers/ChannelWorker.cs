using System.Threading.Channels;

namespace SimpleLauncherEx.Workers;

// このコードはライブラリなので基本変更しない

/// <summary>
/// Channel + Func による直列ワーカー
///
/// - 処理は Enqueue によりキューイングされる
/// - ワーカーは単一スレッドで順次実行される
///
/// 想定用途:
/// - GUI アプリのアプリケーションロジック
/// - ツールアプリのバックグラウンド処理
/// - 状態を持つ非同期ワーカー
///
/// 非想定:
/// - 高並列 CPU 処理
/// - 並列実行が必要な処理
/// </summary>
public sealed class ChannelWorker
{
    // ワーカーで処理するactionの引数。

    // ワーカーに対するリクエストのキュー
    private readonly Channel<Func<Task>> _channel =
        Channel.CreateUnbounded<Func<Task>>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
    /*
        Channel
        Queue<T> を、マルチスレッドや async/await で使いやすく進化させた『上位互換の非同期キュー』
    */
    /*
        Func<Task>
        Func ... デリゲート(関数ポインタみたいなもの)
        Task ... 戻り値、タスクを返す。
    */
    /*
        UnboundedChannelOptions (Channelの設定オプション)
        オプション内容
        SingleReader読み取る側が常に 1 つだけなのでtrue。trueの場合、読み取り処理が最適化されます。
        SingleWriter書き込む側が複数なので false。trueの場合、内部のロックが簡略化され高速になります。
    */

    // コンストラクタ
    public ChannelWorker()
    {
        // ワーカーループの開始
        _ = Task.Run(WorkerLoop);
    }

    // ワーカー（メッセージループみたいなもの）
    private async Task WorkerLoop()
    {
        // _channelにactionが溜まっていたら一つとりだし
        await foreach (var action in _channel.Reader.ReadAllAsync())
        {
            try
            {
                // actionを実行する。
                await action();
            }
            catch (Exception ex)
            {
                // TODO:
                // - ILogger 連携
                // - イベント通知
                // - 統一エラーハンドラ
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
    }

    /// <summary>
    /// 戻り値なし処理をキューに追加
    /// </summary>
    public Task Enqueue(Func<Task> action)
    {
        var tcs = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        /*
          TaskCompletionSource
          SetResult()やSetException()でタスクの終了を元スレッドに伝えることが出来ます。

          TaskCreationOptions.RunContinuationsAsynchronously(オプション)
          結果を元スレッドに通知するだけで、すぐ制御を戻す。
        */

        // _channelに処理を積む
        _channel.Writer.TryWrite(async () =>
        {
            // ラムダ式の内容がワーカーで処理する内容になる。
            try
            {
                // ctx ... コンテキストを引数にactionを実行する。
                await action();
                // 終了を元スレッドへ伝える。
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                // 例外を元スレッドへ伝える。
                tcs.SetException(ex);
            }
        });

        // タスクを返す。
        return tcs.Task;
    }

    /// <summary>
    /// 戻り値あり処理をキューに追加
    /// </summary>
    public Task<TResult> Enqueue<TResult>(
        Func<Task<TResult>> action)
    {
        var tcs = new TaskCompletionSource<TResult>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        _channel.Writer.TryWrite(async () =>
        {
            try
            {
                // actionからの結果をresultで受け取り
                var result = await action();
                // 結果を元スレッドへ伝える。
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        // タスクを返す。
        return tcs.Task;
    }
}
