using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using StudentManagementApp.Models;

namespace StudentManagementApp.Data;

/// <summary>
/// Akses data master: Programs, Students, Courses.
/// Semua SQL ditulis di sini sehingga ViewModel tidak menyentuh SQL secara langsung.
/// </summary>
public class StudentRepository
{
    private readonly string _connectionString;

    public StudentRepository(string? connectionString = null)
    {
        _connectionString = connectionString ?? AppConfig.ConnectionString;
    }

    private SqlConnection CreateConnection() => new(_connectionString);

    // ---------- Programs ----------

    public async Task<List<StudyProgram>> GetProgramsAsync()
    {
        const string sql = @"
            SELECT ProgramId, ProgramCode, ProgramName
            FROM dbo.Programs
            WHERE IsActive = 1
            ORDER BY ProgramCode";

        var result = new List<StudyProgram>();
        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        await conn.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new StudyProgram
            {
                ProgramId = reader.GetInt32(0),
                ProgramCode = reader.GetString(1),
                ProgramName = reader.GetString(2)
            });
        }
        return result;
    }

    public async Task<List<ProgramSummary>> GetProgramSummaryAsync()
    {
        const string sql = @"
            SELECT p.ProgramCode, p.ProgramName, COUNT(s.StudentId) AS StudentCount
            FROM dbo.Programs p
            LEFT JOIN dbo.Students s ON s.ProgramId = p.ProgramId
            GROUP BY p.ProgramCode, p.ProgramName
            ORDER BY StudentCount DESC, p.ProgramCode";

        var result = new List<ProgramSummary>();
        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        await conn.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new ProgramSummary
            {
                ProgramCode = reader.GetString(0),
                ProgramName = reader.GetString(1),
                StudentCount = reader.GetInt32(2)
            });
        }
        return result;
    }

    // ---------- Students ----------

    public async Task<List<Student>> GetStudentsAsync(string? keyword = null)
    {
        const string sql = @"
            SELECT s.StudentId, s.NRP, s.FullName, s.Gender, s.Email, s.EntryYear,
                   s.PhoneNumber, s.ProgramId, p.ProgramCode, s.CreatedAt
            FROM dbo.Students s
            JOIN dbo.Programs p ON p.ProgramId = s.ProgramId
            WHERE @Keyword IS NULL
               OR s.NRP LIKE @Pattern
               OR s.FullName LIKE @Pattern
               OR s.Email LIKE @Pattern
            ORDER BY s.NRP";

        var trimmed = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();

        var result = new List<Student>();
        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Keyword", SqlDbType.NVarChar, 150).Value = (object?)trimmed ?? DBNull.Value;
        cmd.Parameters.Add("@Pattern", SqlDbType.NVarChar, 152).Value = trimmed is null ? DBNull.Value : $"%{trimmed}%";

        await conn.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new Student
            {
                StudentId = reader.GetInt32(0),
                Nrp = reader.GetString(1).Trim(),
                FullName = reader.GetString(2),
                Gender = reader.GetString(3),
                Email = reader.GetString(4),
                EntryYear = reader.GetInt16(5),
                PhoneNumber = reader.IsDBNull(6) ? null : reader.GetString(6),
                ProgramId = reader.GetInt32(7),
                ProgramCode = reader.GetString(8),
                CreatedAt = reader.GetDateTime(9)
            });
        }
        return result;
    }

    public async Task<int> InsertStudentAsync(Student s)
    {
        const string sql = @"
            INSERT INTO dbo.Students (NRP, FullName, Gender, Email, EntryYear, PhoneNumber, ProgramId)
            VALUES (@NRP, @FullName, @Gender, @Email, @EntryYear, @PhoneNumber, @ProgramId)";

        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        AddStudentParameters(cmd, s);
        await conn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> UpdateStudentAsync(Student s)
    {
        const string sql = @"
            UPDATE dbo.Students
            SET NRP = @NRP, FullName = @FullName, Gender = @Gender, Email = @Email,
                EntryYear = @EntryYear, PhoneNumber = @PhoneNumber, ProgramId = @ProgramId,
                UpdatedAt = SYSDATETIME()
            WHERE StudentId = @StudentId";

        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        AddStudentParameters(cmd, s);
        cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = s.StudentId;
        await conn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> DeleteStudentAsync(int studentId)
    {
        const string sql = "DELETE FROM dbo.Students WHERE StudentId = @StudentId";

        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = studentId;
        await conn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }

    private static void AddStudentParameters(SqlCommand cmd, Student s)
    {
        cmd.Parameters.Add("@NRP", SqlDbType.Char, 10).Value = s.Nrp;
        cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 150).Value = s.FullName;
        cmd.Parameters.Add("@Gender", SqlDbType.Char, 1).Value = s.Gender;
        cmd.Parameters.Add("@Email", SqlDbType.VarChar, 150).Value = s.Email;
        cmd.Parameters.Add("@EntryYear", SqlDbType.SmallInt).Value = s.EntryYear;
        cmd.Parameters.Add("@PhoneNumber", SqlDbType.VarChar, 20).Value =
            string.IsNullOrWhiteSpace(s.PhoneNumber) ? DBNull.Value : s.PhoneNumber;
        cmd.Parameters.Add("@ProgramId", SqlDbType.Int).Value = s.ProgramId;
    }

    // ---------- Courses ----------

    public async Task<List<Course>> GetCoursesAsync()
    {
        const string sql = @"
            SELECT CourseId, CourseCode, CourseName, Credits
            FROM dbo.Courses
            ORDER BY Semester, CourseCode";

        var result = new List<Course>();
        await using var conn = CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        await conn.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new Course
            {
                CourseId = reader.GetInt32(0),
                CourseCode = reader.GetString(1),
                CourseName = reader.GetString(2),
                Credits = reader.GetByte(3)
            });
        }
        return result;
    }
}
