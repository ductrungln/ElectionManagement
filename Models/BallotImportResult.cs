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
        
        public int BallotType4Count { get; set; } // Phiếu bầu 4 người
        public int BallotType4Votes { get; set; } // Tổng lượt bầu kiểu 4
        
        // Tổng lượt bầu cho mỗi ứng cử viên
        public int Candidate1Votes { get; set; }
        public int Candidate2Votes { get; set; }
        public int Candidate3Votes { get; set; }
        public int Candidate4Votes { get; set; }
        public int Candidate5Votes { get; set; }
        public int Candidate6Votes { get; set; }
        public int Candidate7Votes { get; set; }
        public int Candidate8Votes { get; set; }

        // Tên của ứng cử viên
        public string Candidate1Name { get; set; }
        public string Candidate2Name { get; set; }
        public string Candidate3Name { get; set; }
        public string Candidate4Name { get; set; }
        public string Candidate5Name { get; set; }
        public string Candidate6Name { get; set; }
        public string Candidate7Name { get; set; }
        public string Candidate8Name { get; set; }
        
        // Total number of candidates (for dynamic form generation)
        public int TotalCandidates { get; set; } = 5;
        
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorDetails { get; set; }
    }
}
