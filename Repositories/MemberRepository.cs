using Dapper;
using Esseti.Data;
using Esseti.Repositories.Interfaces;
using Models.University;
using Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Esseti.Repositories
{
    public class MemberRepository : IMemberRepository
    {
        public async Task<List<Member>> GetAllMembersAsync()
        {
            using (var db = DatabaseConfig.GetConnection())
            {
                var sql = "SELECT m.first_name, m.last_name, m.major, m.join_date, ua.account_id AS AccountID, ua.email, cd.college_department_id AS CollegeDepartmentId, cd.name FROM member m INNER JOIN member_club mc ON m.member_id = mc.member_id INNER JOIN club_info ci ON mc.club_id = ci.club_id INNER JOIN user_account ua ON m.account_id  = ua.account_id INNER JOIN college_department cd ON ci.department_id = cd.college_department_id;";

                var results = await db.QueryAsync<Member, UserAccount, CollegeDepartment, Member>(
                        sql,
                        (member, account, department) =>
                        {
                            member.Account = account;
                            member.Department = department;

                            return member;
                            
                        },
                        splitOn: "AccountId,CollegeDepartmentId"
                    );

                return results.Distinct().ToList();
            }
        }
    }
}