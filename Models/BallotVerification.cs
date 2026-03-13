using System;

namespace ElectionManagement.Models
{
    public class BallotVerification
    {
        public int Id { get; set; }
        public string Level { get; set; } // which level this record belongs to
        public string DistrictName { get; set; }
        public int IssuedBallots { get; set; }           // Số phiếu phát ra
        public int ReceivedBallots { get; set; }         // Số phiếu thu vào
        public int ValidBallots { get; set; }            // Số phiếu hợp lệ
        public int InvalidBallots { get; set; }          // Số phiếu không hợp lệ
        public int BallotType1Count { get; set; }        // Phiếu bầu 1 - Số lượng
        public int BallotType1Votes { get; set; }        // Phiếu bầu 1 - Tổng bầu
        public int BallotType2Count { get; set; }        // Phiếu bầu 2 - Số lượng
        public int BallotType2Votes { get; set; }        // Phiếu bầu 2 - Tổng bầu
        public int BallotType3Count { get; set; }        // Phiếu bầu 3 - Số lượng
        public int BallotType3Votes { get; set; }        // Phiếu bầu 3 - Tổng bầu
        public int Candidate1Votes { get; set; }         // Ứng cử viên 1
        public int Candidate2Votes { get; set; }         // Ứng cử viên 2
        public int Candidate3Votes { get; set; }         // Ứng cử viên 3
        public int Candidate4Votes { get; set; }         // Ứng cử viên 4
        public int Candidate5Votes { get; set; }         // Ứng cử viên 5
        public string Candidate1Name { get; set; }       // Tên ứng cử viên 1
        public string Candidate2Name { get; set; }       // Tên ứng cử viên 2
        public string Candidate3Name { get; set; }       // Tên ứng cử viên 3
        public string Candidate4Name { get; set; }       // Tên ứng cử viên 4
        public string Candidate5Name { get; set; }       // Tên ứng cử viên 5
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
    }
}
