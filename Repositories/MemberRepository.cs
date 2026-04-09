using Dapper;
using Esseti.Data;
using Esseti.Repositories.Interfaces;
using Models.ClubBase;
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
                var sql = @"SELECT 
                        m.first_name, m.last_name, m.major, m.join_date, m.is_active, m.member_avatar, m.index_number,
                        ua.account_id AS AccountID, ua.email, 
                        cd.college_department_id AS CollegeDepartmentId, cd.name AS Name, 
                        ar.role_id AS AuthorityRoleId, ar.name AS Name, 
                        mc.club_id AS MemberClubId, mc.club_role  
                    FROM member m 
                    LEFT JOIN member_club mc ON m.member_id = mc.member_id 
                    LEFT JOIN club_info ci ON mc.club_id = ci.club_id 
                    LEFT JOIN user_account ua ON m.account_id  = ua.account_id 
                    LEFT JOIN college_department cd ON ci.department_id = cd.college_department_id
                    LEFT JOIN authority_role ar ON ar.role_id = m.role_id;";

                var results = await db.QueryAsync<Member, UserAccount, CollegeDepartment, AuthorityRole, MemberClub, Member>(
                        sql,
                        (member, account, department, authorityRole, memberClub) =>
                        {
                            member.Account = account;
                            member.Department = department;
                            member.AuthorityRole = authorityRole;
                            member.MemberClub = memberClub;
                            return member;
                        },
                        splitOn: "AccountID, CollegeDepartmentId, AuthorityRoleId, MemberClubId"
                    );

                return results.Distinct().ToList();
            }
        }
    }
}