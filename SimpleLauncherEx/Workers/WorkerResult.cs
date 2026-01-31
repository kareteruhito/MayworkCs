namespace SimpleLauncherEx.Workers;

// このコードはライブラリなので基本変更しない

/// <summary>
/// ワーカー処理結果を表す汎用結果クラス
///
/// UI層とワーカー層の責務分離のため、
/// 単純な値ではなく Result オブジェクトで返却する。
///
/// 将来的に情報を追加しても API 破壊が起きにくい。
/// </summary>
public sealed class WorkerResult<T>
{
    /// <summary>
    /// 実行結果の値
    /// </summary>
    public required T Value { get; init; }

    /// <summary>
    /// 成功フラグ（未使用でも将来拡張用）
    /// </summary>
    public bool Success { get; init; } = true;

    /// <summary>
    /// UI 表示用メッセージ等
    /// </summary>
    public string? Message { get; init; }

    // 拡張候補:
    // public Exception? Error { get; init; }
    // public object? Tag { get; init; }
}