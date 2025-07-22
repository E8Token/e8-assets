using System;
using Energy8.Identity.Shared.Core.Contracts.Dto.Common;
using Energy8.Identity.Shared.Core.Contracts.Dto.Games;

namespace Game.Dto
{
    /// <summary>
    /// DTO для запроса инициализации игры
    /// </summary>
    [System.Serializable]
    public class GameInitializeRequestDto : DtoBase
    {
        public string SessionId { get; set; }
        
        public GameInitializeRequestDto() { }
        
        public GameInitializeRequestDto(string sessionId)
        {
            SessionId = sessionId;
        }
    }

    /// <summary>
    /// DTO для ответа статуса игры NeonFruits
    /// </summary>
    [System.Serializable]
    public class NFGameStatusResponseDto : DtoBase
    {
        public string SessionId { get; set; }
        public long Balance { get; set; }
        public string Status { get; set; }
        public bool BonusAvailable { get; set; }
        public object GameData { get; set; }
        
        public NFGameStatusResponseDto() { }
        
        public NFGameStatusResponseDto(string sessionId, long balance, string status, bool bonusAvailable = false)
        {
            SessionId = sessionId;
            Balance = balance;
            Status = status;
            BonusAvailable = bonusAvailable;
        }
    }

    /// <summary>
    /// Базовый DTO для запросов спина
    /// </summary>
    [System.Serializable]
    public class SpinRequestDto : DtoBase
    {
        public string SessionId { get; set; }
        public long BetAmount { get; set; }
        public int Lines { get; set; }
        
        public SpinRequestDto() { }
        
        public SpinRequestDto(string sessionId, long betAmount, int lines)
        {
            SessionId = sessionId;
            BetAmount = betAmount;
            Lines = lines;
        }
    }

    /// <summary>
    /// Базовый DTO для ответов статуса игры
    /// </summary>
    [System.Serializable]
    public class GameStatusResponseDto : DtoBase
    {
        public string SessionId { get; set; }
        public long Balance { get; set; }
        public long WinAmount { get; set; }
        public string Status { get; set; }
        
        public GameStatusResponseDto() { }
        
        public GameStatusResponseDto(string sessionId, long balance, long winAmount, string status)
        {
            SessionId = sessionId;
            Balance = balance;
            WinAmount = winAmount;
            Status = status;
        }
    }

    /// <summary>
    /// Конкретная реализация для ответа спина NeonFruits
    /// </summary>
    [System.Serializable]
    public class NeonFruitsSpinResponseDto : GameStatusResponseDto
    {
        public int[] ReelResults { get; set; }
        public string[] WinLines { get; set; }
        public bool FreeSpinsTriggered { get; set; }
        public int FreeSpinsCount { get; set; }
        public string[] Symbols { get; set; }
        
        public NeonFruitsSpinResponseDto() : base() 
        {
            ReelResults = new int[0];
            WinLines = new string[0];
            Symbols = new string[0];
        }
        
        public NeonFruitsSpinResponseDto(string sessionId, long balance, long winAmount, string status, 
            int[] reelResults, string[] winLines, bool freeSpinsTriggered = false, int freeSpinsCount = 0)
            : base(sessionId, balance, winAmount, status)
        {
            ReelResults = reelResults ?? new int[0];
            WinLines = winLines ?? new string[0];
            FreeSpinsTriggered = freeSpinsTriggered;
            FreeSpinsCount = freeSpinsCount;
        }
    }

    /// <summary>
    /// Конкретная реализация запроса спина для NeonFruits
    /// </summary>
    [System.Serializable]
    public class NeonFruitsSpinRequestDto : SpinRequestDto
    {
        public bool AutoPlay { get; set; }
        public int AutoPlayCount { get; set; }
        
        public NeonFruitsSpinRequestDto() : base() { }
        
        public NeonFruitsSpinRequestDto(string sessionId, long betAmount, int lines, bool autoPlay = false, int autoPlayCount = 0)
            : base(sessionId, betAmount, lines)
        {
            AutoPlay = autoPlay;
            AutoPlayCount = autoPlayCount;
        }
    }
}
