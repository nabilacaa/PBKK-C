using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using StudentManagementApp.Models;

namespace StudentManagementApp.Data;

/// <summary>
/// Akses data transaksi KRS (tabel Enrollments) dan view vw_EnrollmentDetail.
/// </summary>
public class EnrollmentRepository
{
    private readonly string _connectionString;

    public EnrollmentRepository(string? connectionString = null)
    {
        _connectionString = connectionString ?? AppConfig.ConnectionString;
    }

    public async Task<List<EnrollmentDetail>> GetEnrollmentsAsync(int? studentId = null)
    {
        const string sql = @"
            SELECT EnrollmentId, StudentId, NRP, FullName, CourseId, CourseCode, CourseName,
                   Credits, AcademicYear, Term, Grade, GradePoint
            FROM dbo.vw_EnrollmentDetail
            WHERE @StudentId IS NULL OR StudentId = @StudentId
            ORDER BY NRP, AcademicYear DESC, Term, CourseCode";

        var result = new List<EnrollmentDetail>();
        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = (object?)studentId ?? DBNull.Value;

        await conn.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new EnrollmentDetail
            {
                EnrollmentId = reader.GetInt32(0),
                StudentId = reader.GetInt32(1),
                Nrp = reader.GetString(2).Trim(),
                FullName = reader.GetString(3),
                CourseId = reader.GetInt32(4),
                CourseCode = reader.GetString(5),
                CourseName = reader.GetString(6),
                Credits = reader.GetByte(7),
                AcademicYear = reader.GetString(8),
                Term = reader.GetString(9),
                Grade = reader.IsDBNull(10) ? null : reader.GetString(10),
                GradePoint = reader.IsDBNull(11) ? null : reader.GetDecimal(11)
            });
        }
        return result;
    }

    public async Task<int> AddEnrollmentAsync(int studentId, int courseId, string academicYear, string term, string? grade)
    {
        const string sql = @"
            INSERT INTO dbo.Enrollments (StudentId, CourseId, AcademicYear, Term, Grade)
            VALUES (@StudentId, @CourseId, @AcademicYear, @Term, @Grade)";

        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = studentId;
        cmd.Parameters.Add("@CourseId", SqlDbType.Int).Value = courseId;
        cmd.Parameters.Add("@AcademicYear", SqlDbType.Char, 9).Value = academicYear;
        cmd.Parameters.Add("@Term", SqlDbType.VarChar, 5).Value = term;
        cmd.Parameters.Add("@Grade", SqlDbType.VarChar, 2).Value = (object?)grade ?? DBNull.Value;

        await conn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> UpdateGradeAsync(int enrollmentId, string? grade)
    {
        const string sql = "UPDATE dbo.Enrollments SET Grade = @Grade WHERE EnrollmentId = @EnrollmentId";

        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Grade", SqlDbType.VarChar, 2).Value = (object?)grade ?? DBNull.Value;
        cmd.Parameters.Add("@EnrollmentId", SqlDbType.Int).Value = enrollmentId;

        await conn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> DeleteEnrollmentAsync(int enrollmentId)
    {
        const string sql = "DELETE FROM dbo.Enrollments WHERE EnrollmentId = @EnrollmentId";

        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@EnrollmentId", SqlDbType.Int).Value = enrollmentId;

        await conn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }
}
