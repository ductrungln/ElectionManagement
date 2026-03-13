using System;

namespace ElectionManagement.Models
{
    public class BallotImportResult
    {
        public int TotalBallots { get; set; }
        public int IssuedBallots { get; set; }
        public int ReceivedBallots { get; set; }
        public int ValidBallots { get; set; }
        public int InvalidBallots { get; set; }
        
        // Phiếu bầu theo số người chọn
        public int BallotType1Count { get; set; } // Phiếu bầu 1 người
        public int BallotType1Votes { get; set; } // Tổng lượt bầu kiểu 1
        
        public int BallotType2Count { get; set; } // Phiếu bầu 2 người
        public int BallotType2Votes { get; set; } // Tổng lượt bầu kiểu 2
        
        public int BallotType3Count { get; set; } // Phiếu bầu 3 người
        public int BallotType3Votes { get; set; } // Tổng lượt bầu kiểu 3
        
        // Tổng lượt bầu cho mỗi ứng cử viên
        public int Candidate1Votes { get; set; }
        public int Candidate2Votes { get; set; }
        public int Candidate3Votes { get; set; }
        public int Candidate4Votes { get; set; }
        public int Candidate5Votes { get; set; }

        // Tên của 5 ứng cử viên
        public string Candidate1Name { get; set; }
        public string Candidate2Name { get; set; }
        public string Candidate3Name { get; set; }
        public string Candidate4Name { get; set; }
        public string Candidate5Name { get; set; }
        
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorDetails { get; set; }
    }
}
