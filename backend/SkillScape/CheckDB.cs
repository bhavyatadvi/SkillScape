using System;
using Microsoft.Data.SqlClient;

class Program {
    static void Main() {
        string connStr = ""Server=(localdb)\\mssqllocaldb;Database=SkillScapeDb;Trusted_Connection=true;"";
        using (var conn = new SqlConnection(connStr)) {
            conn.Open();
            using (var cmd = new SqlCommand(""SELECT Id, PhaseKey, SubPhaseKey FROM QuizQuestionsV2 WHERE Id LIKE 'Q_PC%'"", conn)) {
                using (var reader = cmd.ExecuteReader()) {
                    while(reader.Read()) {
                        Console.WriteLine($""{reader[0]} | {reader[1]} | '{reader[2]}'"");
                    }
                }
            }
        }
    }
}
