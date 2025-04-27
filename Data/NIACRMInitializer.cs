using NIA_CRM.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace NIA_CRM.Data
{
    public static class NIACRMInitializer
    {
        /// <summary>
        /// Prepares the Database and seeds data as required
        /// </summary>
        /// <param name="serviceProvider">DI Container</param>
        /// <param name="DeleteDatabase">Delete the database and start from scratch</param>
        /// <param name="UseMigrations">Use Migrations or EnsureCreated</param>
        /// <param name="SeedSampleData">Add optional sample data</param>
        public static void Initialize(IServiceProvider serviceProvider,
     bool DeleteDatabase = false, bool UseMigrations = true, bool SeedSampleData = true)
        {
            using (var context = new NIACRMContext(
                serviceProvider.GetRequiredService<DbContextOptions<NIACRMContext>>()))
            {
                //Refresh the database as per the parameter options
                #region Prepare the Database
                try
                {
                    //Note: .CanConnect() will return false if the database is not there!
                    if (DeleteDatabase || !context.Database.CanConnect())
                    {
                        context.Database.EnsureDeleted(); //Delete the existing version 
                        if (UseMigrations)
                        {
                            context.Database.Migrate(); //Create the Database and apply all migrations
                        }
                        else
                        {
                            context.Database.EnsureCreated(); //Create and update the database as per the Model
                        }
                        //Now create any additional database objects such as Triggers or Views
                        //--------------------------------------------------------------------
                        //Create the Triggers
                        string sqlCmd = @"
                            CREATE TRIGGER SetProductionEmailTimestampOnUpdate
                            AFTER UPDATE ON ProductionEmails
                            BEGIN
                                UPDATE ProductionEmails
                                SET RowVersion = randomblob(8)
                                WHERE rowid = NEW.rowid;
                            END;
                        ";
                        context.Database.ExecuteSqlRaw(sqlCmd);

                        sqlCmd = @"
                            CREATE TRIGGER SetProductionEmailTimestampOnInsert
                            AFTER INSERT ON ProductionEmails
                            BEGIN
                                UPDATE ProductionEmails
                                SET RowVersion = randomblob(8)
                                WHERE rowid = NEW.rowid;
                            END;
                        ";
                        context.Database.ExecuteSqlRaw(sqlCmd);

                        // Trigger for annual actions
                        string AnnualActionsqlCmd = @"
                            CREATE TRIGGER SetAnnualActionTimestampOnUpdate
                            AFTER UPDATE ON AnnualAction
                            BEGIN
                                UPDATE AnnualAction
                                SET RowVersion = randomblob(8)
                                WHERE rowid = NEW.rowid;
                            END;
                        ";
                        context.Database.ExecuteSqlRaw(AnnualActionsqlCmd);

                        AnnualActionsqlCmd = @"
                            CREATE TRIGGER SetAnnualActionTimestampOnInsert
                            AFTER INSERT ON AnnualAction
                            BEGIN
                                UPDATE AnnualAction
                                SET RowVersion = randomblob(8)
                                WHERE rowid = NEW.rowid;
                            END;
                        ";
                        context.Database.ExecuteSqlRaw(AnnualActionsqlCmd);

                        // Trigger for Cancellations
                        string CancellationsqlCmd = @"
                                CREATE TRIGGER SetCancellationTimestampOnUpdate
                                AFTER UPDATE ON Cancellations
                                BEGIN
                                    UPDATE Cancellations
                                    SET RowVersion = randomblob(8)
                                    WHERE rowid = NEW.rowid;
                                END;
                            ";
                        context.Database.ExecuteSqlRaw(CancellationsqlCmd);

                        CancellationsqlCmd = @"
                                CREATE TRIGGER SetCancellationTimestampOnInsert
                                AFTER INSERT ON Cancellations
                                BEGIN
                                    UPDATE Cancellations
                                    SET RowVersion = randomblob(8)
                                    WHERE rowid = NEW.rowid;
                                END;
                            ";
                        context.Database.ExecuteSqlRaw(CancellationsqlCmd);

                        // Trigger for ContactCancellations - Update
                        string ContactCancellationsqlCmd = @"
                                    CREATE TRIGGER SetContactCancellationTimestampOnUpdate
                                    AFTER UPDATE ON ContactCancellations
                                    BEGIN
                                        UPDATE ContactCancellations
                                        SET RowVersion = randomblob(8)
                                        WHERE rowid = NEW.rowid;
                                    END;
                                ";
                        context.Database.ExecuteSqlRaw(ContactCancellationsqlCmd);

                        // Trigger for ContactCancellations - Insert
                        ContactCancellationsqlCmd = @"
                                    CREATE TRIGGER SetContactCancellationTimestampOnInsert
                                    AFTER INSERT ON ContactCancellations
                                    BEGIN
                                        UPDATE ContactCancellations
                                        SET RowVersion = randomblob(8)
                                        WHERE rowid = NEW.rowid;
                                    END;
                                ";
                        context.Database.ExecuteSqlRaw(ContactCancellationsqlCmd);

                        // Trigger for Contact - Update
                        string ContactsqlCmd = @"
                                    CREATE TRIGGER SetContactTimestampOnUpdate
                                    AFTER UPDATE ON Contacts
                                    BEGIN
                                        UPDATE Contacts
                                        SET RowVersion = randomblob(8)
                                        WHERE rowid = NEW.rowid;
                                    END;
                                ";
                        context.Database.ExecuteSqlRaw(ContactsqlCmd);

                        // Trigger for Contact - Insert
                        ContactsqlCmd = @"
                                    CREATE TRIGGER SetContactTimestampOnInsert
                                    AFTER INSERT ON Contacts
                                    BEGIN
                                        UPDATE Contacts
                                        SET RowVersion = randomblob(8)
                                        WHERE rowid = NEW.rowid;
                                    END;
                                ";
                        context.Database.ExecuteSqlRaw(ContactsqlCmd);

                        // Trigger for Members - Update
                        string MembersqlCmd = @"
                                    CREATE TRIGGER SetMemberTimestampOnUpdate
                                    AFTER UPDATE ON Members
                                    BEGIN
                                        UPDATE Members
                                        SET RowVersion = randomblob(8)
                                        WHERE rowid = NEW.rowid;
                                    END;
                                ";
                        context.Database.ExecuteSqlRaw(MembersqlCmd);

                        // Trigger for Members - Insert
                        MembersqlCmd = @"
                                CREATE TRIGGER SetMemberTimestampOnInsert
                                AFTER INSERT ON Members
                                BEGIN
                                    UPDATE Members
                                    SET RowVersion = randomblob(8)
                                    WHERE rowid = NEW.rowid;
                                END;
                            ";
                        context.Database.ExecuteSqlRaw(MembersqlCmd);

                        // Trigger for MEvents - Update
                        string MEventssqlCmd = @"
                                    CREATE TRIGGER SetMEventTimestampOnUpdate
                                    AFTER UPDATE ON MEvents
                                    BEGIN
                                        UPDATE MEvents
                                        SET RowVersion = randomblob(8)
                                        WHERE rowid = NEW.rowid;
                                    END;
                                ";
                        context.Database.ExecuteSqlRaw(MEventssqlCmd);

                        // Trigger for MEvents - Insert
                        MEventssqlCmd = @"
                                    CREATE TRIGGER SetMEventTimestampOnInsert
                                    AFTER INSERT ON MEvents
                                    BEGIN
                                        UPDATE MEvents
                                        SET RowVersion = randomblob(8)
                                        WHERE rowid = NEW.rowid;
                                    END;
                                ";
                        context.Database.ExecuteSqlRaw(MEventssqlCmd);

                        // Trigger for Opportunities - Update
                        string OpportunitiesqlCmd = @"
                                    CREATE TRIGGER SetOpportunityTimestampOnUpdate
                                    AFTER UPDATE ON Opportunities
                                    BEGIN
                                        UPDATE Opportunities
                                        SET RowVersion = randomblob(8)
                                        WHERE rowid = NEW.rowid;
                                    END;
                                ";
                        context.Database.ExecuteSqlRaw(OpportunitiesqlCmd);

                        // Trigger for Opportunities - Insert
                        OpportunitiesqlCmd = @"
                                    CREATE TRIGGER SetOpportunityTimestampOnInsert
                                    AFTER INSERT ON Opportunities
                                    BEGIN
                                        UPDATE Opportunities
                                        SET RowVersion = randomblob(8)
                                        WHERE rowid = NEW.rowid;
                                    END;
                                ";
                        context.Database.ExecuteSqlRaw(OpportunitiesqlCmd);

                        // Trigger for Strategy - Update
                        string StrategySqlCmd = @"
                                    CREATE TRIGGER SetStrategyTimestampOnUpdate
                                    AFTER UPDATE ON Strategy
                                    BEGIN
                                        UPDATE Strategy
                                        SET RowVersion = randomblob(8)
                                        WHERE rowid = NEW.rowid;
                                    END;
                                ";
                        context.Database.ExecuteSqlRaw(StrategySqlCmd);

                        // Trigger for Strategy - Insert
                        StrategySqlCmd = @"
                                    CREATE TRIGGER SetStrategyTimestampOnInsert
                                    AFTER INSERT ON Strategy
                                    BEGIN
                                        UPDATE Strategy
                                        SET RowVersion = randomblob(8)
                                        WHERE rowid = NEW.rowid;
                                    END;
                                ";
                        context.Database.ExecuteSqlRaw(StrategySqlCmd);

                        // Trigger for Sector - Update
                        string SectorSqlCmd = @"
                                    CREATE TRIGGER SetSectorTimestampOnUpdate
                                    AFTER UPDATE ON Sectors
                                    BEGIN
                                        UPDATE Sectors
                                        SET RowVersion = randomblob(8)
                                        WHERE rowid = NEW.rowid;
                                    END;
                                ";
                        context.Database.ExecuteSqlRaw(SectorSqlCmd);

                        // Trigger for Sector - Insert
                        SectorSqlCmd = @"
                                    CREATE TRIGGER SetSectorTimestampOnInsert
                                    AFTER INSERT ON Sectors
                                    BEGIN
                                        UPDATE Sectors
                                        SET RowVersion = randomblob(8)
                                        WHERE rowid = NEW.rowid;
                                    END;
                                ";
                        context.Database.ExecuteSqlRaw(SectorSqlCmd);


                        // Trigger for MTag - Update
                        string MTagSqlCmd = @"
                                    CREATE TRIGGER SetMTagTimestampOnUpdate
                                    AFTER UPDATE ON MTag
                                    BEGIN
                                        UPDATE MTag
                                        SET RowVersion = randomblob(8)
                                        WHERE rowid = NEW.rowid;
                                    END;
                                ";
                        context.Database.ExecuteSqlRaw(MTagSqlCmd);

                        // Trigger for MTag - Insert
                        MTagSqlCmd = @"
                                    CREATE TRIGGER SetMTagTimestampOnInsert
                                    AFTER INSERT ON MTag
                                    BEGIN
                                        UPDATE MTag
                                        SET RowVersion = randomblob(8)
                                        WHERE rowid = NEW.rowid;
                                    END;
                                ";
                        context.Database.ExecuteSqlRaw(MTagSqlCmd);

                        // Trigger for MembershipTypes - Update
                        string MembershipTypesSqlCmd = @"
                                    CREATE TRIGGER SetMembershipTypeTimestampOnUpdate
                                    AFTER UPDATE ON MembershipTypes
                                    BEGIN
                                        UPDATE MembershipTypes
                                        SET RowVersion = randomblob(8)
                                        WHERE rowid = NEW.rowid;
                                    END;
                                ";
                        context.Database.ExecuteSqlRaw(MembershipTypesSqlCmd);

                        // Trigger for MembershipTypes - Insert
                        MembershipTypesSqlCmd = @"
                                    CREATE TRIGGER SetMembershipTypeTimestampOnInsert
                                    AFTER INSERT ON MembershipTypes
                                    BEGIN
                                        UPDATE MembershipTypes
                                        SET RowVersion = randomblob(8)
                                        WHERE rowid = NEW.rowid;
                                    END;
                                ";
                        context.Database.ExecuteSqlRaw(MembershipTypesSqlCmd);

                    }
                    else //The database is already created
                    {
                        if (UseMigrations)
                        {
                            context.Database.Migrate(); //Apply all migrations
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Database setup error: {ex.GetBaseException().Message}");
                    throw;
                }

                #endregion

                #region Seed Sample Data
                try
                {
                    //Add some Class Start times

                    if (!context.MembershipTypes.Any())
                    {
                        context.MembershipTypes.AddRange(
                            new MembershipType
                            {
                                Id = 1,
                                TypeName = "Associate",
                                TypeDescr = "Access to gym equipment and locker room facilities."
                            },
                            new MembershipType
                            {
                                Id = 2,
                                TypeName = "Chamber,Associate",
                                TypeDescr = "Includes Basic Membership benefits plus access to group classes and pool."
                            },
                            new MembershipType
                            {
                                Id = 3,
                                TypeName = "Government & Education,Associate",
                                TypeDescr = "Includes Premium Membership benefits for up to 4 family members."
                            },
                            new MembershipType
                            {
                                Id = 4,
                                TypeName = "Local Industrial",
                                TypeDescr = "Discounted membership for students with valId Id."
                            },
                            new MembershipType
                            {
                                Id = 5,
                                TypeName = "Non-Local Industrial",
                                TypeDescr = "Special membership for employees of partner organizations."
                            }
                        );
                        context.SaveChanges();
                    }

                    if (!context.Members.Any())
                    {
                        context.Members.AddRange(
                            new Member
                            {

                                ID = 1,
                                MemberName = "Alpha Steel",
                                MemberSize = 10,
                                JoinDate = new DateTime(2021, 1, 1),
                                WebsiteUrl = "https://www.johndoe.com",
                                Address = new Address // Updated to one-to-one relationship
                                {
                                    AddressLine1 = "123 Main St",
                                    AddressLine2 = "Apt 1B",
                                    City = "Niagara Falls",
                                    StateProvince = Province.Ontario,
                                    PostalCode = "L2G 3Y7"
                                }
                            },

                        new Member
                        {
                            ID = 2,
                            MemberName = "TISCO CO.",
                            MemberSize = 5,
                            JoinDate = new DateTime(2020, 6, 15),
                            WebsiteUrl = "https://www.janesmith.com",
                            Address = new Address // Updated to one-to-one relationship
                            {
                                AddressLine1 = "123 Main St",
                                AddressLine2 = "Apt 1B",
                                City = "Niagara Falls",
                                StateProvince = Province.Ontario,
                                PostalCode = "L2G 3Y7"
                            }

                        },

                        new Member
                        {
                            ID = 3,
                            MemberName = "M Time Irons",
                            MemberSize = 8,
                            JoinDate = new DateTime(2019, 4, 21),
                            WebsiteUrl = "https://www.emilyjohnson.com",
                            Address = new Address // Updated to one-to-one relationship
                            {

                                AddressLine1 = "789 Pine Rd",
                                AddressLine2 = "Suite 3C",
                                City = "Niagara Falls",
                                StateProvince = Province.Ontario,
                                PostalCode = "L2E 6S5"
                            }

                        },
                        new Member
                        {
                            ID = 4,
                            MemberName = "Forge & Foundry Inc.",
                            MemberSize = 6,
                            JoinDate = new DateTime(2021, 7, 11),
                            WebsiteUrl = "https://www.michaelbrown.com",
                            Address = new Address // Updated to one-to-one relationship
                            {

                                AddressLine1 = "101 Maple St",
                                AddressLine2 = "Apt 4D",
                                City = "Niagara Falls",
                                StateProvince = Province.Ontario,
                                PostalCode = "L2E 1B1"

                            }
                        },
                        new Member
                        {
                            ID = 5,
                            MemberName = "Northern Metalworks",
                            MemberSize = 4,
                            JoinDate = new DateTime(2020, 3, 25),
                            WebsiteUrl = "https://www.sarahdavis.com",
                            Address = new Address // Updated to one-to-one relationship
                            {

                                AddressLine1 = "555 Birch Blvd",
                                AddressLine2 = "Unit 7B",
                                City = "Niagara Falls",
                                StateProvince = Province.Ontario,
                                PostalCode = "L2G 7M7"

                            }
                        },
                        new Member
                        {
                            ID = 6,
                            MemberName = "Titanium Solutions",
                            MemberSize = 12,
                            JoinDate = new DateTime(2022, 5, 19),
                            WebsiteUrl = "https://www.davidmartinez.com",
                            Address = new Address // Updated to one-to-one relationship
                            {

                                AddressLine1 = "888 Cedar St",
                                AddressLine2 = "Apt 10E",
                                City = "St. Catharines",
                                StateProvince = Province.Ontario,
                                PostalCode = "L2M 3Y3"
                            }
                        },
                        new Member
                        {
                            ID = 7,
                            MemberName = "Phoenix Alloys",
                            MemberSize = 15,
                            JoinDate = new DateTime(2018, 2, 7),
                            WebsiteUrl = "https://www.robertwilson.com",
                            Address = new Address // Updated to one-to-one relationship
                            {

                                AddressLine1 = "222 Elm St",
                                AddressLine2 = "Suite 5A",
                                City = "St. Catharines",
                                StateProvince = Province.Ontario,
                                PostalCode = "L2P 3H2"

                            }
                        },
                        new Member
                        {
                            ID = 8,
                            MemberName = "Galaxy Metals",
                            MemberSize = 3,
                            JoinDate = new DateTime(2020, 8, 30),
                            WebsiteUrl = "https://www.williammoore.com",
                            Address = new Address // Updated to one-to-one relationship
                            {

                                AddressLine1 = "333 Ash Ave",
                                AddressLine2 = "Unit 2C",
                                City = "St. Catharines",
                                StateProvince = Province.Ontario,
                                PostalCode = "L2N 5V4"

                            }
                        },
                        new Member
                        {
                            ID = 9,
                            MemberName = "Ironclad Industries",
                            MemberSize = 9,
                            JoinDate = new DateTime(2022, 10, 18),
                            WebsiteUrl = "https://www.oliviataylor.com",
                            Address = new Address // Updated to one-to-one relationship
                            {
                                AddressLine1 = "444 Birch Rd",
                                AddressLine2 = "Suite 5B",
                                City = "St. Catharines",
                                StateProvince = Province.Ontario,
                                PostalCode = "L2T 2P3"

                            }
                        },
                        new Member
                        {
                            ID = 10,
                            MemberName = "Silverline Fabrication",
                            MemberSize = 7,
                            JoinDate = new DateTime(2019, 5, 21),
                            WebsiteUrl = "https://www.sophiaanderson.com",
                            Address = new Address // Updated to one-to-one relationship
                            {
                                AddressLine1 = "555 Oak Blvd",
                                AddressLine2 = "Unit 1A",
                                City = "St. Catharines",
                                StateProvince = Province.Ontario,
                                PostalCode = "L2S 1P9"
                            }
                        },
                        new Member
                        {
                            ID = 11,
                            MemberName = "Star Steelworks",
                            MemberSize = 2,
                            JoinDate = new DateTime(2021, 12, 8),
                            WebsiteUrl = "https://www.jamesthomas.com",
                            Address = new Address // Updated to one-to-one relationship
                            {
                                AddressLine1 = "111 Maple Rd",
                                AddressLine2 = "Apt 2C",
                                City = "Welland",
                                StateProvince = Province.Ontario,
                                PostalCode = "L3B 1A1"
                            }
                        },
                        new Member
                        {
                            ID = 12,
                            MemberName = "Summit Metal Co.",
                            MemberSize = 11,
                            JoinDate = new DateTime(2017, 11, 15),
                            WebsiteUrl = "https://www.daniellee.com",
                            Address = new Address // Updated to one-to-one relationship
                            {
                                AddressLine1 = "333 Pine Blvd",
                                AddressLine2 = "Apt 1F",
                                City = "Welland",
                                StateProvince = Province.Ontario,
                                PostalCode = "L3B 5N9"
                            }
                        },
                        new Member
                        {
                            ID = 13,
                            MemberName = "Everest Iron Corp.",
                            MemberSize = 8,
                            JoinDate = new DateTime(2022, 9, 22),
                            WebsiteUrl = "https://www.lucasharris.com",
                            Address = new Address // Updated to one-to-one relationship
                            {
                                AddressLine1 = "777 Oak Rd",
                                AddressLine2 = "Suite 5B",
                                City = "Welland",
                                StateProvince = Province.Ontario,
                                PostalCode = "L3C 7C1"
                            }
                        },
                        new Member
                        {
                            ID = 14,
                            MemberName = "Prime Alloy Coatings",
                            MemberSize = 6,
                            JoinDate = new DateTime(2022, 6, 30),
                            WebsiteUrl = "https://www.ellawalker.com",
                            Address = new Address // Updated to one-to-one relationship
                            {
                                AddressLine1 = "555 Birch St",
                                AddressLine2 = "Unit 4A",
                                City = "Welland",
                                StateProvince = Province.Ontario,
                                PostalCode = "L3C 4T6"
                            }
                        },
                        new Member
                        {
                            ID = 15,
                            MemberName = "Magnum Steel Solutions",
                            MemberSize = 4,
                            JoinDate = new DateTime(2021, 3, 12),
                            WebsiteUrl = "https://www.liamrobinson.com",
                            Address = new Address // Updated to one-to-one relationship
                            {
                                AddressLine1 = "200 Maple Rd",
                                AddressLine2 = "Unit 6D",
                                City = "Welland",
                                StateProvince = Province.Ontario,
                                PostalCode = "L3C 2A9"
                            }
                        },
                        new Member
                        {
                            ID = 16,
                            MemberName = "Quantum Tech Innovations",
                            MemberSize = 4,
                            JoinDate = new DateTime(2021, 11, 12),
                            WebsiteUrl = "https://www.avalewis.com",
                            Address = new Address // Updated to one-to-one relationship
                            {
                                AddressLine1 = "965 Elm St",
                                AddressLine2 = "Apt 7A",
                                City = "Thorold",
                                StateProvince = Province.Ontario,
                                PostalCode = "L2V 4Y6"
                            }
                        },
                        new Member
                        {
                            ID = 17,
                            MemberName = "Aurora Renewable Energy",
                            MemberSize = 3,
                            JoinDate = new DateTime(2020, 4, 28),
                            WebsiteUrl = "https://www.ethanwalker.com",
                            Address = new Address // Updated to one-to-one relationship
                            {

                                AddressLine1 = "124 Maple Blvd",
                                AddressLine2 = "Unit 6",
                                City = "Thorold",
                                StateProvince = Province.Ontario,
                                PostalCode = "L2V 1H1"
                            }


                        },
                            new Member
                            {
                                ID = 18,
                                MemberName = "Vertex Financial Group",
                                MemberSize = 6,
                                JoinDate = new DateTime(2021, 10, 4),
                                WebsiteUrl = "https://www.masonking.com",
                                Address = new Address // Updated to one-to-one relationship
                                {
                                    AddressLine1 = "481 Cedar Ave",
                                    AddressLine2 = "Apt 12B",
                                    City = "Thorold",
                                    StateProvince = Province.Ontario,
                                    PostalCode = "L2V 3P2"

                                }
                            },
                             new Member
                             {
                                 ID = 19,
                                 MemberName = "Nova Biotech Labs",
                                 MemberSize = 4,
                                 JoinDate = new DateTime(2022, 4, 18),
                                 WebsiteUrl = "https://www.lucasgreen.com",
                                 Address = new Address // Updated to one-to-one relationship
                                 {
                                     AddressLine1 = "187 Birch Rd",
                                     AddressLine2 = "Suite 4",
                                     City = "Thorold",
                                     StateProvince = Province.Ontario,
                                     PostalCode = "L2V 5Z8"
                                 }
                             },
                              new Member
                              {
                                  ID = 20,
                                  MemberName = "Summit Construction Co.",
                                  MemberSize = 9,
                                  JoinDate = new DateTime(2021, 7, 14),
                                  WebsiteUrl = "https://www.charlottehall.com",
                                  Address = new Address // Updated to one-to-one relationship
                                  {
                                      AddressLine1 = "922 Cedar St",
                                      AddressLine2 = "Unit 5A",
                                      City = "Thorold",
                                      StateProvince = Province.Ontario,
                                      PostalCode = "L2V 4K9"
                                  }
                              },
                               new Member
                               {
                                   ID = 21,
                                   MemberName = "Oceanic Shipping Corp",
                                   MemberSize = 7,
                                   JoinDate = new DateTime(2021, 8, 30),
                                   WebsiteUrl = "https://www.benjaminharris.com",
                                   Address = new Address // Updated to one-to-one relationship
                                   {
                                       AddressLine1 = "643 Cedar Blvd",
                                       AddressLine2 = "Apt 9D",
                                       City = "Port Colborne",
                                       StateProvince = Province.Ontario,
                                       PostalCode = "L3K 2W9"
                                   }
                               },
                                new Member
                                {
                                    ID = 22,
                                    MemberName = "Evergreen Agriculture",
                                    MemberSize = 2,
                                    JoinDate = new DateTime(2022, 9, 7),
                                    WebsiteUrl = "https://www.aidenclark.com",
                                    Address = new Address // Updated to one-to-one relationship
                                    {
                                        AddressLine1 = "876 Maple Rd",
                                        AddressLine2 = "Unit 1B",
                                        City = "Port Colborne",
                                        StateProvince = Province.Ontario,
                                        PostalCode = "L3K 3V2"
                                    }
                                },
                                new Member
                                {
                                    ID = 23,
                                    MemberName = "Ironclad Manufacturing Ltd.",
                                    MemberSize = 3,
                                    JoinDate = new DateTime(2020, 12, 15),
                                    WebsiteUrl = "https://www.ellamoore.com",
                                    Address = new Address // Updated to one-to-one relationship
                                    {

                                        AddressLine1 = "134 Pine St",
                                        AddressLine2 = "Apt 6A",
                                        City = "Port Colborne",
                                        StateProvince = Province.Ontario,
                                        PostalCode = "L3K 6A9"
                                    }
                                },
                                 new Member
                                 {
                                     ID = 24,
                                     MemberName = "Skyline Architects Inc.",
                                     MemberSize = 5,
                                     JoinDate = new DateTime(2021, 5, 21),
                                     WebsiteUrl = "https://www.jacobwhite.com",
                                     Address = new Address // Updated to one-to-one relationship
                                     {
                                         AddressLine1 = "789 Oak St",
                                         AddressLine2 = "Suite 4B",
                                         City = "Port Colborne",
                                         StateProvince = Province.Ontario,
                                         PostalCode = "L3K 5E8"
                                     }
                                 },
                                  new Member
                                  {
                                      ID = 25,
                                      MemberName = "Pinnacle Consulting Services",
                                      MemberSize = 8,
                                      JoinDate = new DateTime(2020, 10, 18),
                                      WebsiteUrl = "https://www.abigailnelson.com",
                                      Address = new Address // Updated to one-to-one relationship
                                      {
                                          AddressLine1 = "233 Cedar St",
                                          AddressLine2 = "Unit 7C",
                                          City = "Port Colborne",
                                          StateProvince = Province.Ontario,
                                          PostalCode = "L3K 7X5"
                                      }
                                  },
                                  new Member
                                  {
                                      ID = 26,
                                      MemberName = "Crystal Water Solutions",
                                      MemberSize = 6,
                                      JoinDate = new DateTime(2021, 7, 14),
                                      WebsiteUrl = "https://www.masonlee.com",
                                      Address = new Address // Updated to one-to-one relationship
                                      {
                                          AddressLine1 = "111 Birch Blvd",
                                          AddressLine2 = "Apt 4D",
                                          City = "Grimsby",
                                          StateProvince = Province.Ontario,
                                          PostalCode = "L3M 1R2"
                                      }
                                  },
                                  new Member
                                  {
                                      ID = 27,
                                      MemberName = "Elite Healthcare Partners",
                                      MemberSize = 9,
                                      JoinDate = new DateTime(2022, 1, 22),
                                      WebsiteUrl = "https://www.chloescott.com",
                                      Address = new Address // Updated to one-to-one relationship
                                      {
                                          AddressLine1 = "533 Cedar Rd",
                                          AddressLine2 = "Unit 2B",
                                          City = "Grimsby",
                                          StateProvince = Province.Ontario,
                                          PostalCode = "L3M 4N6"
                                      }
                                  },
                                  new Member
                                  {
                                      ID = 28,
                                      MemberName = "Galaxy IT Solutions",
                                      MemberSize = 7,
                                      JoinDate = new DateTime(2022, 7, 11),
                                      WebsiteUrl = "https://www.danielharris.com",
                                      Address = new Address // Updated to one-to-one relationship
                                      {
                                          AddressLine1 = "987 Maple St",
                                          AddressLine2 = "Apt 3A",
                                          City = "Grimsby",
                                          StateProvince = Province.Ontario,
                                          PostalCode = "L3M 3J5"
                                      }
                                  },
                                  new Member
                                  {
                                      ID = 29,
                                      MemberName = "Urban Infrastructure Group",
                                      MemberSize = 4,
                                      JoinDate = new DateTime(2021, 9, 3),
                                      WebsiteUrl = "https://www.avacarter.com",
                                      Address = new Address // Updated to one-to-one relationship
                                      {
                                          AddressLine1 = "444 Oak Blvd",
                                          AddressLine2 = "Suite 8B",
                                          City = "Grimsby",
                                          StateProvince = Province.Ontario,
                                          PostalCode = "L3M 2A8"
                                      }
                                  },
                                   new Member
                                   {
                                       ID = 30,
                                       MemberName = "Horizon Aerospace Inc.",
                                       MemberSize = 5,
                                       JoinDate = new DateTime(2022, 8, 18),
                                       WebsiteUrl = "https://www.landonwalker.com",
                                       Address = new Address // Updated to one-to-one relationship
                                       {
                                           AddressLine1 = "872 Cedar Rd",
                                           AddressLine2 = "Apt 9C",
                                           City = "Grimsby",
                                           StateProvince = Province.Ontario,
                                           PostalCode = "L3M 5K9"
                                       }
                                   },
                                   new Member
                                   {
                                       ID = 31,
                                       MemberName = "Cobalt Mining Ventures",
                                       MemberSize = 6,
                                       JoinDate = new DateTime(2022, 6, 13),
                                       WebsiteUrl = "https://www.ameliaharris.com",
                                       Address = new Address // Updated to one-to-one relationship
                                       {
                                           AddressLine1 = "1234 Oak Blvd",
                                           AddressLine2 = "Apt 2C",
                                           City = "Fort Erie",
                                           StateProvince = Province.Ontario,
                                           PostalCode = "L2A 5R1"
                                       }
                                   },
                                   new Member
                                   {
                                       ID = 32,
                                       MemberName = "Lakeside Resorts and Hotels",
                                       MemberSize = 9,
                                       JoinDate = new DateTime(2022, 7, 6),
                                       WebsiteUrl = "https://www.oliverlee.com",
                                       Address = new Address // Updated to one-to-one relationship
                                       {
                                           AddressLine1 = "990 Pine Rd",
                                           AddressLine2 = "Unit 7",
                                           City = "Fort Erie",
                                           StateProvince = Province.Ontario,
                                           PostalCode = "L2A 7B9"
                                       }
                                   },
                                   new Member
                                   {
                                       ID = 33,
                                       MemberName = "NextGen Media Productions",
                                       MemberSize = 4,
                                       JoinDate = new DateTime(2021, 5, 10),
                                       WebsiteUrl = "https://www.harperscott.com",
                                       Address = new Address // Updated to one-to-one relationship
                                       {
                                           AddressLine1 = "522 Cedar Rd",
                                           AddressLine2 = "Suite 6A",
                                           City = "Fort Erie",
                                           StateProvince = Province.Ontario,
                                           PostalCode = "L2A 2T6"
                                       }
                                   },
                                   new Member
                                   {
                                       ID = 34,
                                       MemberName = "Crestwood Pharmaceutical",
                                       MemberSize = 7,
                                       JoinDate = new DateTime(2022, 1, 24),
                                       WebsiteUrl = "https://www.sophieadams.com",
                                       Address = new Address // Updated to one-to-one relationship
                                       {
                                           AddressLine1 = "690 Birch St",
                                           AddressLine2 = "Unit 3A",
                                           City = "Fort Erie",
                                           StateProvince = Province.Ontario,
                                           PostalCode = "L2A 9W8"
                                       }
                                   },
                                   new Member
                                   {
                                       ID = 35,
                                       MemberName = "Dynamic Logistics Group",
                                       MemberSize = 2,
                                       JoinDate = new DateTime(2021, 4, 9),
                                       WebsiteUrl = "https://www.isaacmorgan.com",
                                       Address = new Address // Updated to one-to-one relationship
                                       {
                                           AddressLine1 = "123 Birch Rd",
                                           AddressLine2 = "Apt 7C",
                                           City = "Fort Erie",
                                           StateProvince = Province.Ontario,
                                           PostalCode = "L2A 4K3"
                                       }
                                   },
                                   new Member
                                   {
                                       ID = 36,
                                       MemberName = "Northern Timber Products",
                                       MemberSize = 8,
                                       JoinDate = new DateTime(2022, 4, 19),
                                       WebsiteUrl = "https://www.miathompson.com",
                                       Address = new Address // Updated to one-to-one relationship
                                       {
                                           AddressLine1 = "987 Birch Rd",
                                           AddressLine2 = "Unit 4A",
                                           City = "Lincoln",
                                           StateProvince = Province.Ontario,
                                           PostalCode = "L0R 1B1"
                                       }
                                   },
                                   new Member
                                   {
                                       ID = 37,
                                       MemberName = "Brightline Education Systems",
                                       MemberSize = 5,
                                       JoinDate = new DateTime(2022, 10, 10),
                                       WebsiteUrl = "https://www.ethanjohnson.com",
                                       Address = new Address // Updated to one-to-one relationship
                                       {
                                           AddressLine1 = "456 Oak Rd",
                                           AddressLine2 = "Suite 2B",
                                           City = "Lincoln",
                                           StateProvince = Province.Ontario,
                                           PostalCode = "L0R 2C0"
                                       }
                                   },
                                           new Member
                                           {
                                               ID = 38,
                                               MemberName = "Fusion Energy Solutions",
                                               MemberSize = 4,
                                               JoinDate = new DateTime(2022, 5, 15),
                                               WebsiteUrl = "https://www.gracemiller.com",
                                               Address = new Address // Updated to one-to-one relationship
                                               {
                                                   AddressLine1 = "890 Cedar Blvd",
                                                   AddressLine2 = "Suite 2B",
                                                   City = "Pelham",
                                                   StateProvince = Province.Ontario,
                                                   PostalCode = "L0S 1C0"
                                               }
                                           },
                                           new Member
                                           {
                                               ID = 39,
                                               MemberName = "Trailblazer Automotive Group",
                                               MemberSize = 3,
                                               JoinDate = new DateTime(2022, 8, 22),
                                               WebsiteUrl = "https://www.lilyturner.com",
                                               Address = new Address // Updated to one-to-one relationship
                                               {
                                                   AddressLine1 = "800 Maple Blvd",
                                                   AddressLine2 = "Unit 5A",
                                                   City = "Pelham",
                                                   StateProvince = Province.Ontario,
                                                   PostalCode = "L0S 1E0"
                                               }
                                           },
                                           new Member
                                           {
                                               ID = 40,
                                               MemberName = "Harvest Foods International",
                                               MemberSize = 6,
                                               JoinDate = new DateTime(2021, 3, 18),
                                               WebsiteUrl = "https://www.liamwalker.com",
                                               Address = new Address // Updated to one-to-one relationship
                                               {
                                                   AddressLine1 = "354 Cedar St",
                                                   AddressLine2 = "Apt 6D",
                                                   City = "Fort Erie",
                                                   StateProvince = Province.Ontario,
                                                   PostalCode = "L2A 1M7"
                                               }

                                           },
                                           new Member
                                           {
                                               ID = 41,
                                               MemberName = "Niagara Energy Solutions",
                                               MemberSize = 10,
                                               JoinDate = new DateTime(2020, 9, 12),
                                               WebsiteUrl = "https://www.jacobpeterson.com",
                                               Address = new Address // Updated to one-to-one relationship
                                               {
                                                   AddressLine1 = "123 Power Ave",
                                                   AddressLine2 = "Suite 1B",
                                                   City = "Niagara Falls",
                                                   StateProvince = Province.Ontario,
                                                   PostalCode = "L2E 3P2"
                                               }
                                           },
                                        new Member
                                        {
                                            ID = 42,
                                            MemberName = "Brock University",
                                            MemberSize = 25,
                                            JoinDate = new DateTime(2021, 5, 3),
                                            WebsiteUrl = "https://www.marcusjones.com",
                                            Address = new Address // Updated to one-to-one relationship
                                            {
                                                AddressLine1 = "500 Glenridge Ave",
                                                AddressLine2 = "Building C",
                                                City = "St. Catharines",
                                                StateProvince = Province.Ontario,
                                                PostalCode = "L2S 3A1"
                                            }
                                        },
                                        new Member
                                        {
                                            ID = 43,
                                            MemberName = "Niagara Healthcare Inc.",
                                            MemberSize = 15,
                                            JoinDate = new DateTime(2022, 1, 20),
                                            WebsiteUrl = "https://www.sarahmartin.com",
                                            Address = new Address // Updated to one-to-one relationship
                                            {
                                                AddressLine1 = "45 Welland Ave",
                                                AddressLine2 = "Unit 7B",
                                                City = "Welland",
                                                StateProvince = Province.Ontario,
                                                PostalCode = "L3C 1V8"
                                            }
                                        },
                                        new Member
                                        {
                                            ID = 44,
                                            MemberName = "Niagara Financial Advisors",
                                            MemberSize = 20,
                                            JoinDate = new DateTime(2021, 8, 14),
                                            WebsiteUrl = "https://www.oliviamartinez.com",
                                            Address = new Address // Updated to one-to-one relationship
                                            {
                                                AddressLine1 = "2500 South Service Rd",
                                                AddressLine2 = "Suite 11A",
                                                City = "Grimsby",
                                                StateProvince = Province.Ontario,
                                                PostalCode = "L3M 2R7"
                                            }
                                        },
                                        new Member
                                        {
                                            ID = 45,
                                            MemberName = "Vineyard Estates Winery",
                                            MemberSize = 30,
                                            JoinDate = new DateTime(2022, 4, 8),
                                            WebsiteUrl = "https://www.tylermorris.com",
                                            Address = new Address // Updated to one-to-one relationship
                                            {
                                                AddressLine1 = "1234 Vine St",
                                                AddressLine2 = "Winery Rd",
                                                City = "Niagara-on-the-Lake",
                                                StateProvince = Province.Ontario,
                                                PostalCode = "L0S 1J0"
                                            }
                                        },
                                        new Member
                                        {
                                            ID = 46,
                                            MemberName = "St. Catharines Brewing Co.",
                                            MemberSize = 8,
                                            JoinDate = new DateTime(2020, 11, 2),
                                            WebsiteUrl = "https://www.danielcollins.com",
                                            Address = new Address // Updated to one-to-one relationship
                                            {
                                                AddressLine1 = "29 Queen St",
                                                AddressLine2 = "Brewery Lane",
                                                City = "St. Catharines",
                                                StateProvince = Province.Ontario,
                                                PostalCode = "L2R 5A9"
                                            }
                                        },
                                        new Member
                                        {
                                            ID = 47,
                                            MemberName = "Niagara Logistics Solutions",
                                            MemberSize = 12,
                                            JoinDate = new DateTime(2021, 10, 10),
                                            WebsiteUrl = "https://www.rachelharris.com",
                                            Address = new Address // Updated to one-to-one relationship
                                            {
                                                AddressLine1 = "55 Industrial Dr",
                                                AddressLine2 = "Unit 3",
                                                City = "Thorold",
                                                StateProvince = Province.Ontario,
                                                PostalCode = "L2V 2P9"
                                            }
                                        },
                                        new Member
                                        {
                                            ID = 48,
                                            MemberName = "Niagara Roofing & Construction",
                                            MemberSize = 10,
                                            JoinDate = new DateTime(2022, 2, 11),
                                            WebsiteUrl = "https://www.jamieclark.com",
                                            Address = new Address // Updated to one-to-one relationship
                                            {
                                                AddressLine1 = "2141 Mewburn Rd",
                                                AddressLine2 = "Suite 10",
                                                City = "Niagara Falls",
                                                StateProvince = Province.Ontario,
                                                PostalCode = "L2G 7V6"
                                            }
                                        },
                                        new Member
                                        {
                                            ID = 49,
                                            MemberName = "The Port Colborne Bakery",
                                            MemberSize = 5,
                                            JoinDate = new DateTime(2021, 6, 17),
                                            WebsiteUrl = "https://www.nicholasanderson.com",
                                            Address = new Address // Updated to one-to-one relationship
                                            {
                                                AddressLine1 = "1500 Elm St",
                                                AddressLine2 = "Unit 4",
                                                City = "Port Colborne",
                                                StateProvince = Province.Ontario,
                                                PostalCode = "L3K 5Y5"
                                            }
                                        },
                                        new Member
                                        {
                                            ID = 50,
                                            MemberName = "Sunset Motors",
                                            MemberSize = 7,
                                            JoinDate = new DateTime(2022, 5, 19),
                                            WebsiteUrl = "https://www.elizabethlee.com",
                                            Address = new Address // Updated to one-to-one relationship
                                            {
                                                AddressLine1 = "123 Sunset Blvd",
                                                AddressLine2 = "Car Sales",
                                                City = "Niagara Falls",
                                                StateProvince = Province.Ontario,
                                                PostalCode = "L2E 6X5"
                                            }
                                        },
                                        new Member
                                        {
                                            ID = 51,
                                            MemberName = "Niagara Recycling Ltd.",
                                            MemberSize = 22,
                                            JoinDate = new DateTime(2021, 12, 25),
                                            WebsiteUrl = "https://www.nicholasjones.com",
                                            Address = new Address // Updated to one-to-one relationship
                                            {
                                                AddressLine1 = "7893 South Niagara Pkwy",
                                                AddressLine2 = "Recycling Plant",
                                                City = "Niagara Falls",
                                                StateProvince = Province.Ontario,
                                                PostalCode = "L2E 6V8"
                                            }
                                        },
                                        new Member
                                        {
                                            ID = 52,
                                            MemberName = "Rosewood Estates Winery",
                                            MemberSize = 20,
                                            JoinDate = new DateTime(2022, 9, 18),
                                            WebsiteUrl = "https://www.meganvaughn.com",
                                            Address = new Address // Updated to one-to-one relationship
                                            {
                                                AddressLine1 = "123 Rosewood Ave",
                                                AddressLine2 = "Winery Rd",
                                                City = "Niagara-on-the-Lake",
                                                StateProvince = Province.Ontario,
                                                PostalCode = "L0S 1J1"
                                            }
                                        },
                                        new Member
                                        {
                                            ID = 53,
                                            MemberName = "Niagara Falls Convention Centre",
                                            MemberSize = 18,
                                            JoinDate = new DateTime(2020, 7, 10),
                                            WebsiteUrl = "https://www.andrewjohnson.com",
                                            Address = new Address // Updated to one-to-one relationship
                                            {
                                                AddressLine1 = "6815 Stanley Ave",
                                                AddressLine2 = "Convention Centre",
                                                City = "Niagara Falls",
                                                StateProvince = Province.Ontario,
                                                PostalCode = "L2G 3Y9"
                                            }
                                        },
                                        new Member
                                        {
                                            ID = 54,
                                            MemberName = "Niagara Home Furnishings",
                                            MemberSize = 14,
                                            JoinDate = new DateTime(2021, 4, 25),
                                            WebsiteUrl = "https://www.joshuasmith.com",
                                            Address = new Address // Updated to one-to-one relationship
                                            {
                                                AddressLine1 = "7600 Montrose Rd",
                                                AddressLine2 = "Furniture Store",
                                                City = "Niagara Falls",
                                                StateProvince = Province.Ontario,
                                                PostalCode = "L2H 2T7"
                                            }
                                        },
                                        new Member
                                        {
                                            ID = 55,
                                            MemberName = "Niagara Construction Ltd.",
                                            MemberSize = 12,
                                            JoinDate = new DateTime(2020, 10, 14),
                                            WebsiteUrl = "https://www.justinwhite.com",
                                            Address = new Address // Updated to one-to-one relationship
                                            {
                                                AddressLine1 = "1550 Ontario St",
                                                AddressLine2 = "Unit 9",
                                                City = "St. Catharines",
                                                StateProvince = Province.Ontario,
                                                PostalCode = "L2N 7Y4"
                                            }
                                        },
                                        new Member
                                        {
                                            ID = 56,
                                            MemberName = "Brockway Plumbing Services",
                                            MemberSize = 6,
                                            JoinDate = new DateTime(2021, 2, 7),
                                            WebsiteUrl = "https://www.jennifermartin.com",
                                            Address = new Address // Updated to one-to-one relationship
                                            {
                                                AddressLine1 = "9800 Lundy's Lane",
                                                AddressLine2 = "Unit 12",
                                                City = "Niagara Falls",
                                                StateProvince = Province.Ontario,
                                                PostalCode = "L2H 1H7"
                                            }
                                        },
                                        new Member
                                        {
                                            ID = 57,
                                            MemberName = "Grand Niagara Golf Club",
                                            MemberSize = 25,
                                            JoinDate = new DateTime(2021, 11, 16),
                                            WebsiteUrl = "https://www.kimberlydavis.com",
                                            Address = new Address // Updated to one-to-one relationship
                                            {
                                                AddressLine1 = "1500 Montrose Rd",
                                                AddressLine2 = "Golf Club",
                                                City = "Niagara Falls",
                                                StateProvince = Province.Ontario,
                                                PostalCode = "L2H 3N6"
                                            }
                                        },
                                        new Member
                                        {
                                            ID = 58,
                                            MemberName = "Niagara Peninsula Foods",
                                            MemberSize = 28,
                                            JoinDate = new DateTime(2020, 12, 5),
                                            WebsiteUrl = "https://www.ryanscott.com",
                                            Address = new Address // Updated to one-to-one relationship
                                            {
                                                AddressLine1 = "6347 Stanley Ave",
                                                AddressLine2 = "Unit 20",
                                                City = "Niagara Falls",
                                                StateProvince = Province.Ontario,
                                                PostalCode = "L2G 3Z6"
                                            }
                                        },
                                        new Member
                                        {
                                            ID = 59,
                                            MemberName = "Niagara Falls Hospitality Group",
                                            MemberSize = 30,
                                            JoinDate = new DateTime(2021, 5, 10),
                                            WebsiteUrl = "https://www.dylanross.com",
                                            Address = new Address // Updated to one-to-one relationship
                                            {
                                                AddressLine1 = "123 Victoria Ave",
                                                AddressLine2 = "Hospitality Suite",
                                                City = "Niagara Falls",
                                                StateProvince = Province.Ontario,
                                                PostalCode = "L2E 4Y3"
                                            }
                                        },
                                        new Member
                                        {
                                            ID = 60,
                                            MemberName = "Peachland Grocers",
                                            MemberSize = 14,
                                            JoinDate = new DateTime(2022, 3, 29),
                                            WebsiteUrl = "https://www.johnadams.com",
                                            Address = new Address // Updated to one-to-one relationship
                                            {
                                                AddressLine1 = "8255 Greenhill Ave",
                                                AddressLine2 = "Grocery Store",
                                                City = "Pelham",
                                                StateProvince = Province.Ontario,
                                                PostalCode = "L0S 1E2"
                                            }
                                        },
                                        new Member
                                        {
                                            ID = 61,
                                            MemberName = "Niagara Valley Distillery",
                                            MemberSize = 8,
                                            JoinDate = new DateTime(2022, 8, 30),
                                            WebsiteUrl = "https://www.alexthompson.com",
                                            Address = new Address // Updated to one-to-one relationship
                                            {
                                                AddressLine1 = "1904 Niagara Stone Rd",
                                                AddressLine2 = "Distillery Lane",
                                                City = "Niagara-on-the-Lake",
                                                StateProvince = Province.Ontario,
                                                PostalCode = "L0S 1J0"
                                            }
                                        },
                                    new Member
                                    {
                                        ID = 62,
                                        MemberName = "The House of Jerky",
                                        MemberSize = 6,
                                        JoinDate = new DateTime(2021, 3, 5),
                                        WebsiteUrl = "https://www.susanwilliams.com",
                                        Address = new Address // Updated to one-to-one relationship
                                        {
                                            AddressLine1 = "120 Main St E",
                                            AddressLine2 = "Unit 3",
                                            City = "Grimsby",
                                            StateProvince = Province.Ontario,
                                            PostalCode = "L3M 1P3"
                                        }
                                    },
                                    new Member
                                    {
                                        ID = 63,
                                        MemberName = "Port Niagara Supplies",
                                        MemberSize = 12,
                                        JoinDate = new DateTime(2020, 12, 8),
                                        WebsiteUrl = "https://www.nathanharris.com",
                                        Address = new Address // Updated to one-to-one relationship
                                        {
                                            AddressLine1 = "500 Port Rd",
                                            AddressLine2 = "Warehouse 4",
                                            City = "Port Colborne",
                                            StateProvince = Province.Ontario,
                                            PostalCode = "L3K 3T2"
                                        }
                                    },
                                    new Member
                                    {
                                        ID = 64,
                                        MemberName = "Hamilton Fabricators",
                                        MemberSize = 18,
                                        JoinDate = new DateTime(2021, 7, 22),
                                        WebsiteUrl = "https://www.davidmiller.com",
                                        Address = new Address // Updated to one-to-one relationship
                                        {
                                            AddressLine1 = "8200 Park Rd",
                                            AddressLine2 = "Steelworks Building",
                                            City = "Stoney Creek",
                                            StateProvince = Province.Ontario,
                                            PostalCode = "L8E 5R2"
                                        }
                                    },
                                    new Member
                                    {
                                        ID = 65,
                                        MemberName = "Niagara Freight Forwarders",
                                        MemberSize = 16,
                                        JoinDate = new DateTime(2021, 9, 18),
                                        WebsiteUrl = "https://www.kendalljohnson.com",
                                        Address = new Address // Updated to one-to-one relationship
                                        {
                                            AddressLine1 = "2173 Merrittville Hwy",
                                            AddressLine2 = "Freight Office",
                                            City = "Thorold",
                                            StateProvince = Province.Ontario,
                                            PostalCode = "L2V 1A1"
                                        }
                                    },
                                    new Member
                                    {
                                        ID = 66,
                                        MemberName = "Fort Erie Construction Ltd.",
                                        MemberSize = 14,
                                        JoinDate = new DateTime(2022, 4, 5),
                                        WebsiteUrl = "https://www.michaelscott.com",
                                        Address = new Address // Updated to one-to-one relationship
                                        {
                                            AddressLine1 = "2567 Dominion Rd",
                                            AddressLine2 = "Building A",
                                            City = "Fort Erie",
                                            StateProvince = Province.Ontario,
                                            PostalCode = "L2A 1E5"
                                        }
                                    },
                                    new Member
                                    {
                                        ID = 67,
                                        MemberName = "Niagara Water Services",
                                        MemberSize = 9,
                                        JoinDate = new DateTime(2020, 11, 12),
                                        WebsiteUrl = "https://www.lucasbrown.com",
                                        Address = new Address // Updated to one-to-one relationship
                                        {
                                            AddressLine1 = "7600 South Service Rd",
                                            AddressLine2 = "Water Distribution Centre",
                                            City = "Grimsby",
                                            StateProvince = Province.Ontario,
                                            PostalCode = "L3M 2Z1"
                                        }
                                    },
                                    new Member
                                    {
                                        ID = 68,
                                        MemberName = "Summit Sports Equipment",
                                        MemberSize = 13,
                                        JoinDate = new DateTime(2022, 2, 2),
                                        WebsiteUrl = "https://www.sophiareid.com",
                                        Address = new Address // Updated to one-to-one relationship
                                        {
                                            AddressLine1 = "1450 Mountain Rd",
                                            AddressLine2 = "Sporting Goods Store",
                                            City = "Niagara Falls",
                                            StateProvince = Province.Ontario,
                                            PostalCode = "L2G 1X9"
                                        }
                                    },
                                    new Member
                                    {
                                        ID = 69,
                                        MemberName = "Niagara Waterpark Group",
                                        MemberSize = 24,
                                        JoinDate = new DateTime(2021, 6, 15),
                                        WebsiteUrl = "https://www.annaevans.com",
                                        Address = new Address // Updated to one-to-one relationship
                                        {
                                            AddressLine1 = "2001 Park Rd",
                                            AddressLine2 = "Waterpark Entrance",
                                            City = "Niagara Falls",
                                            StateProvince = Province.Ontario,
                                            PostalCode = "L2E 6T1"
                                        }
                                    },
                                    new Member
                                    {
                                        ID = 70,
                                        MemberName = "Niagara Adventure Tours",
                                        MemberSize = 8,
                                        JoinDate = new DateTime(2022, 7, 25),
                                        WebsiteUrl = "https://www.williamroberts.com",
                                        Address = new Address // Updated to one-to-one relationship
                                        {
                                            AddressLine1 = "1786 Falls Ave",
                                            AddressLine2 = "Tour Operator HQ",
                                            City = "Niagara Falls",
                                            StateProvince = Province.Ontario,
                                            PostalCode = "L2E 6V9"
                                        }
                                    },
                                    new Member
                                    {
                                        ID = 71,
                                        MemberName = "Kettle Creek Logistics",
                                        MemberSize = 20,
                                        JoinDate = new DateTime(2022, 3, 18),
                                        WebsiteUrl = "https://www.ryanjames.com",
                                        Address = new Address // Updated to one-to-one relationship
                                        {
                                            AddressLine1 = "8459 Creek Rd",
                                            AddressLine2 = "Logistics Centre",
                                            City = "Niagara-on-the-Lake",
                                            StateProvince = Province.Ontario,
                                            PostalCode = "L0S 1J0"
                                        }
                                    },
                                    new Member
                                    {
                                        ID = 72,
                                        MemberName = "Niagara Custom Carpentry",
                                        MemberSize = 12,
                                        JoinDate = new DateTime(2021, 1, 28),
                                        WebsiteUrl = "https://www.emilydavis.com",
                                        Address = new Address // Updated to one-to-one relationship
                                        {
                                            AddressLine1 = "3125 Highway 20",
                                            AddressLine2 = "Woodworking Shop",
                                            City = "Thorold",
                                            StateProvince = Province.Ontario,
                                            PostalCode = "L2V 3M4"
                                        }
                                    },
                                    new Member
                                    {
                                        ID = 73,
                                        MemberName = "Greenstone Landscaping",
                                        MemberSize = 7,
                                        JoinDate = new DateTime(2022, 5, 8),
                                        WebsiteUrl = "https://www.sophiebaker.com",
                                        Address = new Address // Updated to one-to-one relationship
                                        {
                                            AddressLine1 = "1349 Greenstone Rd",
                                            AddressLine2 = "Landscaping Services",
                                            City = "St. Catharines",
                                            StateProvince = Province.Ontario,
                                            PostalCode = "L2M 3W3"
                                        }
                                    },
                                    new Member
                                    {
                                        ID = 74,
                                        MemberName = "Niagara Marketing Group",
                                        MemberSize = 10,
                                        JoinDate = new DateTime(2021, 4, 1),
                                        WebsiteUrl = "https://www.kennethgonzalez.com",
                                        Address = new Address // Updated to one-to-one relationship
                                        {
                                            AddressLine1 = "1550 King St",
                                            AddressLine2 = "Marketing Agency",
                                            City = "Niagara Falls",
                                            StateProvince = Province.Ontario,
                                            PostalCode = "L2G 1J7"
                                        }
                                    },
                                    new Member
                                    {
                                        ID = 75,
                                        MemberName = "Elmwood Construction",
                                        MemberSize = 8,
                                        JoinDate = new DateTime(2022, 5, 15),
                                        WebsiteUrl = "https://www.alexanderjohnson.com",
                                        Address = new Address // Updated to one-to-one relationship
                                        {
                                            AddressLine1 = "500 Elmwood Ave",
                                            AddressLine2 = "Construction Office",
                                            City = "Welland",
                                            StateProvince = Province.Ontario,
                                            PostalCode = "L3C 1W1"
                                        }
                                    },
                                    new Member
                                    {
                                        ID = 76,
                                        MemberName = "Harbourview Cafe",
                                        MemberSize = 4,
                                        JoinDate = new DateTime(2021, 11, 2),
                                        WebsiteUrl = "https://www.jonathandoe.com",
                                        Address = new Address // Updated to one-to-one relationship
                                        {
                                            AddressLine1 = "5100 Lakeshore Rd",
                                            AddressLine2 = "Cafe Front",
                                            City = "Port Colborne",
                                            StateProvince = Province.Ontario,
                                            PostalCode = "L3K 5V3"
                                        }
                                    },
                                    new Member
                                    {
                                        ID = 77,
                                        MemberName = "Niagara River Cruises",
                                        MemberSize = 15,
                                        JoinDate = new DateTime(2021, 10, 11),
                                        WebsiteUrl = "https://www.isaacmoore.com",
                                        Address = new Address // Updated to one-to-one relationship
                                        {
                                            AddressLine1 = "123 Niagara Pkwy",
                                            AddressLine2 = "Cruise Dock",
                                            City = "Niagara-on-the-Lake",
                                            StateProvince = Province.Ontario,
                                            PostalCode = "L0S 1J0"
                                        }
                                    },
                                    new Member
                                    {
                                        ID = 78,
                                        MemberName = "Main Street Bistro",
                                        MemberSize = 6,
                                        JoinDate = new DateTime(2021, 9, 5),
                                        WebsiteUrl = "https://www.maryjackson.com",
                                        Address = new Address // Updated to one-to-one relationship
                                        {
                                            AddressLine1 = "435 Main St",
                                            AddressLine2 = "Bistro Shop",
                                            City = "Grimsby",
                                            StateProvince = Province.Ontario,
                                            PostalCode = "L3M 1P1"
                                        }
                                    },
                                    new Member
                                    {
                                        ID = 79,
                                        MemberName = "Clearwater Springs Lodge",
                                        MemberSize = 28,
                                        JoinDate = new DateTime(2021, 12, 15),
                                        WebsiteUrl = "https://www.olivermiller.com",
                                        Address = new Address // Updated to one-to-one relationship
                                        {
                                            AddressLine1 = "2252 Lakeshore Rd",
                                            AddressLine2 = "Lodge Entrance",
                                            City = "Fort Erie",
                                            StateProvince = Province.Ontario,
                                            PostalCode = "L2A 1G2"
                                        }
                                    },
                                    new Member
                                    {
                                        ID = 80,
                                        MemberName = "The Stone Oven Bakery",
                                        MemberSize = 5,
                                        JoinDate = new DateTime(2022, 1, 17),
                                        WebsiteUrl = "https://www.jamesroberts.com",
                                        Address = new Address // Updated to one-to-one relationship
                                        {
                                            AddressLine1 = "100 Main St",
                                            AddressLine2 = "Bakery Front",
                                            City = "Niagara-on-the-Lake",
                                            StateProvince = Province.Ontario,
                                            PostalCode = "L0S 1J0"
                                        }
                                    },
                                    new Member
                                    {
                                        ID = 81,
                                        MemberName = "Golden Oaks Winery",
                                        MemberSize = 11,
                                        JoinDate = new DateTime(2022, 2, 10),
                                        WebsiteUrl = "https://www.jenniferhill.com",
                                        Address = new Address // Updated to one-to-one relationship
                                        {
                                            AddressLine1 = "1234 Golden Rd",
                                            AddressLine2 = "Tasting Room",
                                            City = "Niagara-on-the-Lake",
                                            StateProvince = Province.Ontario,
                                            PostalCode = "L0S 1J0"
                                        }
                                    },
                                new Member
                                {
                                    ID = 82,
                                    MemberName = "Cedar Ridge Rentals",
                                    MemberSize = 10,
                                    JoinDate = new DateTime(2021, 8, 19),
                                    WebsiteUrl = "https://www.joemartinez.com",
                                    Address = new Address // Updated to one-to-one relationship
                                    {
                                        AddressLine1 = "1000 Ridge Rd",
                                        AddressLine2 = "Rental Office",
                                        City = "St. Catharines",
                                        StateProvince = Province.Ontario,
                                        PostalCode = "L2P 3R3"
                                    }
                                },
                                new Member
                                {
                                    ID = 83,
                                    MemberName = "Silverstone Golf Club",
                                    MemberSize = 35,
                                    JoinDate = new DateTime(2021, 5, 14),
                                    WebsiteUrl = "https://www.christopheranderson.com",
                                    Address = new Address // Updated to one-to-one relationship
                                    {
                                        AddressLine1 = "500 Silverstone Dr",
                                        AddressLine2 = "Clubhouse",
                                        City = "Niagara Falls",
                                        StateProvince = Province.Ontario,
                                        PostalCode = "L2E 6V1"
                                    }
                                },
                                new Member
                                {
                                    ID = 84,
                                    MemberName = "Napa Valley Art Gallery",
                                    MemberSize = 6,
                                    JoinDate = new DateTime(2022, 1, 23),
                                    WebsiteUrl = "https://www.briannawilliams.com",
                                    Address = new Address // Updated to one-to-one relationship
                                    {
                                        AddressLine1 = "222 Art St",
                                        AddressLine2 = "Gallery Showroom",
                                        City = "Niagara-on-the-Lake",
                                        StateProvince = Province.Ontario,
                                        PostalCode = "L0S 1J0"
                                    }
                                },
                                new Member
                                {
                                    ID = 85,
                                    MemberName = "Waterfront Bites Restaurant",
                                    MemberSize = 15,
                                    JoinDate = new DateTime(2021, 10, 2),
                                    WebsiteUrl = "https://www.daniellawson.com",
                                    Address = new Address // Updated to one-to-one relationship
                                    {
                                        AddressLine1 = "345 Lake Rd",
                                        AddressLine2 = "Restaurant Dining",
                                        City = "Port Colborne",
                                        StateProvince = Province.Ontario,
                                        PostalCode = "L3K 3Y6"
                                    }
                                },
                                new Member
                                {
                                    ID = 86,
                                    MemberName = "Starlight Cinemas",
                                    MemberSize = 50,
                                    JoinDate = new DateTime(2020, 11, 28),
                                    WebsiteUrl = "https://www.ashleymorris.com",
                                    Address = new Address // Updated to one-to-one relationship
                                    {
                                        AddressLine1 = "2200 Main St",
                                        AddressLine2 = "Cinema Entrance",
                                        City = "Niagara Falls",
                                        StateProvince = Province.Ontario,
                                        PostalCode = "L2G 1J4"
                                    }
                                },
                                new Member
                                {
                                    ID = 87,
                                    MemberName = "Niagara Fitness Club",
                                    MemberSize = 30,
                                    JoinDate = new DateTime(2021, 6, 30),
                                    WebsiteUrl = "https://www.jessicaperez.com",
                                    Address = new Address // Updated to one-to-one relationship
                                    {
                                        AddressLine1 = "1585 Fitness Rd",
                                        AddressLine2 = "Gym Entrance",
                                        City = "St. Catharines",
                                        StateProvince = Province.Ontario,
                                        PostalCode = "L2R 1C9"
                                    }
                                },
                                new Member
                                {
                                    ID = 88,
                                    MemberName = "Rolling Hills Construction",
                                    MemberSize = 20,
                                    JoinDate = new DateTime(2022, 3, 10),
                                    WebsiteUrl = "https://www.jordanpeterson.com",
                                    Address = new Address // Updated to one-to-one relationship
                                    {

                                        AddressLine1 = "1196 Rolling Hills Rd",
                                        AddressLine2 = "Construction Site",
                                        City = "Welland",
                                        StateProvince = Province.Ontario,
                                        PostalCode = "L3B 4K9"
                                    }
                                },
                                new Member
                                {
                                    ID = 89,
                                    MemberName = "Firefly Electronics",
                                    MemberSize = 10,
                                    JoinDate = new DateTime(2021, 4, 25),
                                    WebsiteUrl = "https://www.marykline.com",
                                    Address = new Address // Updated to one-to-one relationship
                                    {
                                        AddressLine1 = "789 Tech Rd",
                                        AddressLine2 = "Electronics HQ",
                                        City = "Grimsby",
                                        StateProvince = Province.Ontario,
                                        PostalCode = "L3M 4R2"
                                    }
                                },
                                new Member
                                {
                                    ID = 90,
                                    MemberName = "Crystal Clear Pools",
                                    MemberSize = 18,
                                    JoinDate = new DateTime(2022, 6, 15),
                                    WebsiteUrl = "https://www.nicholasjohnson.com",
                                    Address = new Address // Updated to one-to-one relationship
                                    {
                                        AddressLine1 = "1200 Crystal Blvd",
                                        AddressLine2 = "Pool Services",
                                        City = "Niagara Falls",
                                        StateProvince = Province.Ontario,
                                        PostalCode = "L2E 1P8"
                                    }
                                },
                                new Member
                                {
                                    ID = 91,
                                    MemberName = "Niagara Soapworks",
                                    MemberSize = 8,
                                    JoinDate = new DateTime(2022, 4, 18),
                                    WebsiteUrl = "https://www.lauranorris.com",
                                    Address = new Address // Updated to one-to-one relationship
                                    {

                                        AddressLine1 = "234 Soap Rd",
                                        AddressLine2 = "Soap Factory",
                                        City = "Niagara-on-the-Lake",
                                        StateProvince = Province.Ontario,
                                        PostalCode = "L0S 1J0"
                                    }
                                },
                                new Member
                                {
                                    ID = 92,
                                    MemberName = "Pinehurst Brewing Company",
                                    MemberSize = 20,
                                    JoinDate = new DateTime(2021, 9, 23),
                                    WebsiteUrl = "https://www.hannahbrooks.com",
                                    Address = new Address // Updated to one-to-one relationship
                                    {
                                        AddressLine1 = "567 Pinehurst Rd",
                                        AddressLine2 = "Brewery Entrance",
                                        City = "Thorold",
                                        StateProvince = Province.Ontario,
                                        PostalCode = "L2V 1A9"

                                    }
                                },
                                new Member
                                {
                                    ID = 93,
                                    MemberName = "Niagara Ice Creamery",
                                    MemberSize = 5,
                                    JoinDate = new DateTime(2021, 12, 20),
                                    WebsiteUrl = "https://www.danielpatel.com",
                                    Address = new Address // Updated to one-to-one relationship
                                    {

                                        AddressLine1 = "1346 Creamery Rd",
                                        AddressLine2 = "Ice Cream Shop",
                                        City = "Niagara Falls",
                                        StateProvince = Province.Ontario,
                                        PostalCode = "L2E 6T3"
                                    }
                                },
                                new Member
                                {
                                    ID = 94,
                                    MemberName = "Green Valley Farms",
                                    MemberSize = 14,
                                    JoinDate = new DateTime(2022, 2, 28),
                                    WebsiteUrl = "https://www.amandaevans.com",
                                    Address = new Address // Updated to one-to-one relationship
                                    {
                                        AddressLine1 = "2345 Green Valley Rd",
                                        AddressLine2 = "Farm Shop",
                                        City = "St. Catharines",
                                        StateProvince = Province.Ontario,
                                        PostalCode = "L2P 3J5"
                                    }
                                },
                                new Member
                                {
                                    ID = 95,
                                    MemberName = "Lakefront Wine Cellars",
                                    MemberSize = 10,
                                    JoinDate = new DateTime(2021, 8, 12),
                                    WebsiteUrl = "https://www.rachelwhitman.com",
                                    Address = new Address // Updated to one-to-one relationship
                                    {

                                        AddressLine1 = "665 Lakeview Rd",
                                        AddressLine2 = "Tasting Room",
                                        City = "Niagara-on-the-Lake",
                                        StateProvince = Province.Ontario,
                                        PostalCode = "L0S 1J0"
                                    }
                                },
                                new Member
                                {
                                    ID = 96,
                                    MemberName = "Vineyard View Estates",
                                    MemberSize = 25,
                                    JoinDate = new DateTime(2022, 5, 7),
                                    WebsiteUrl = "https://www.oliviagray.com",
                                    Address = new Address // Updated to one-to-one relationship
                                    {

                                        AddressLine1 = "888 Vineyard Dr",
                                        AddressLine2 = "Winery Entrance",
                                        City = "Niagara-on-the-Lake",
                                        StateProvince = Province.Ontario,
                                        PostalCode = "L0S 1J0"
                                    }
                                },
                            new Member
                            {
                                ID = 97,
                                MemberName = "Pinewood Resorts",
                                MemberSize = 40,
                                JoinDate = new DateTime(2021, 11, 15),
                                WebsiteUrl = "https://www.juliajones.com",
                                Address = new Address // Updated to one-to-one relationship
                                {

                                    AddressLine1 = "1420 Pinewood Ln",
                                    AddressLine2 = "Resort Main Office",
                                    City = "Welland",
                                    StateProvince = Province.Ontario,
                                    PostalCode = "L3B 2H6"

                                }
                            },
                            new Member
                            {
                                ID = 98,
                                MemberName = "Riverside Marinas",
                                MemberSize = 12,
                                JoinDate = new DateTime(2022, 8, 1),
                                WebsiteUrl = "https://www.matthewharris.com",
                                Address = new Address // Updated to one-to-one relationship
                                {
                                    AddressLine1 = "750 Riverside Dr",
                                    AddressLine2 = "Marina Office",
                                    City = "Port Colborne",
                                    StateProvince = Province.Ontario,
                                    PostalCode = "L3K 5C3"
                                }
                            },
                            new Member
                            {
                                ID = 99,
                                MemberName = "Niagara Craft Distillery",
                                MemberSize = 8,
                                JoinDate = new DateTime(2021, 7, 18),
                                WebsiteUrl = "https://www.kaylathompson.com",
                                Address = new Address // Updated to one-to-one relationship
                                {

                                    AddressLine1 = "346 Distillery Rd",
                                    AddressLine2 = "Distillery Shop",
                                    City = "Niagara-on-the-Lake",
                                    StateProvince = Province.Ontario,
                                    PostalCode = "L0S 1J0"
                                }
                            },
                            new Member
                            {
                                ID = 100,
                                MemberName = "Heritage Hotels & Resorts",
                                MemberSize = 65,
                                JoinDate = new DateTime(2022, 3, 5),
                                WebsiteUrl = "https://www.zoemorris.com",
                                Address = new Address // Updated to one-to-one relationship
                                {

                                    AddressLine1 = "200 Heritage Ln",
                                    AddressLine2 = "Hotel Main Entrance",
                                    City = "Niagara Falls",
                                    StateProvince = Province.Ontario,
                                    PostalCode = "L2G 1P8"
                                }

                            }

                        );


                        context.SaveChanges();
                    }

                    if (!context.MemberMembershipTypes.Any())
                    {
                        // MembershipTypeIds (from 1 to 5, corresponding to different types)
                        int[] membershipTypeIds = new int[] { 1, 2, 3, 4, 5 }; // Basic, Premium, Family, Student, Corporate

                        // Create a random generator
                        Random random = new Random();

                        // Initialize a list to hold the MemberMembershipType entries
                        List<MemberMembershipType> memberMembershipTypes = new List<MemberMembershipType>();

                        // Loop over the member IDs (from 1 to 100)
                        for (int memberId = 1; memberId <= 100; memberId++)
                        {
                            // Randomly pick a MembershipTypeId for each member
                            int randomMembershipTypeId = membershipTypeIds[random.Next(membershipTypeIds.Length)];

                            // Add the random membership type to the list
                            memberMembershipTypes.Add(new MemberMembershipType
                            {
                                MemberId = memberId,
                                MembershipTypeId = randomMembershipTypeId
                            });
                        }

                        // Save the data to the context
                        context.MemberMembershipTypes.AddRange(memberMembershipTypes);
                        context.SaveChanges();
                    }

                    if (!context.Contacts.Any())
                    {
                        context.Contacts.AddRange(
                            new Contact
                            {
                                Id = 1,
                                FirstName = "John",
                                LastName = "Thomas",
                                Title = "Manager",
                                Department = "Sales",
                                Email = "john.doe@example.com",
                                Phone = "1234567890",
                                LinkedInUrl = "https://www.linkedin.com/in/johndoe",
                                IsVip = true
                            },
                            new Contact
                            {
                                Id = 2,
                                FirstName = "Jane",
                                LastName = "Smith",
                                Title = "Director",
                                Department = "Marketing",
                                Email = "jane.smith@example.com",
                                Phone = "9876543210",
                                LinkedInUrl = "https://www.linkedin.com/in/janesmith",
                                IsVip = false
                            },
                            new Contact
                            {
                                Id = 3,
                                FirstName = "Alice",
                                LastName = "Johnson",
                                Title = "VP",
                                Department = "Human Resources",
                                Email = "alice.johnson@example.com",
                                Phone = "5551234567",
                                LinkedInUrl = "https://www.linkedin.com/in/alicejohnson",
                                IsVip = true
                            },
                            new Contact
                            {
                                Id = 4,
                                FirstName = "Bob",
                                LastName = "Joe",
                                Title = "Chief Financial Officer",
                                Department = "Finance",
                                Email = "bob.brown@example.com",
                                Phone = "5557654321",
                                LinkedInUrl = "https://www.linkedin.com/in/bobbrown",
                                IsVip = true
                            },
                            new Contact
                            {
                                Id = 5,
                                FirstName = "Charlie",
                                LastName = "Davis",
                                Title = "Chief Operating Officer",
                                Department = "Operations",
                                Email = "charlie.davis@example.com",
                                Phone = "5557890123",
                                LinkedInUrl = "https://www.linkedin.com/in/charliedavis",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 6,
                                FirstName = "Deborah",
                                LastName = "Williams",
                                Title = "Director of Technology",
                                Department = "Technology",
                                Email = "deborah.williams@example.com",
                                Phone = "5552345678",
                                LinkedInUrl = "https://www.linkedin.com/in/deborahwilliams",
                                IsVip = true,
                            },
                            new Contact
                            {
                                Id = 7,
                                FirstName = "Eve",
                                LastName = "Marie",
                                Title = "Marketing Specialist",
                                Department = "Marketing",
                                Email = "eve.taylor@example.com",
                                Phone = "5553456789",
                                LinkedInUrl = "https://www.linkedin.com/in/evetaylor",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 8,
                                FirstName = "Frank",
                                LastName = "Harris",
                                Title = "Senior Engineer",
                                Department = "Engineering",
                                Email = "frank.harris@example.com",
                                Phone = "5554567890",
                                LinkedInUrl = "https://www.linkedin.com/in/frankharris",
                                IsVip = true,
                            },
                            new Contact
                            {
                                Id = 9,
                                FirstName = "Grace",
                                LastName = "King",
                                Title = "Business Development Manager",
                                Department = "Sales",
                                Email = "grace.king@example.com",
                                Phone = "5555678901",
                                LinkedInUrl = "https://www.linkedin.com/in/graceking",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 10,
                                FirstName = "Hank",
                                LastName = "Lee",
                                Title = "Head of Research",
                                Department = "Research and Development",
                                Email = "hank.lee@example.com",
                                Phone = "5556789012",
                                LinkedInUrl = "https://www.linkedin.com/in/hanklee",
                                IsVip = true,
                            },
                            new Contact
                            {
                                Id = 11,
                                FirstName = "Ivy",
                                LastName = "Adams",
                                Title = "Project Manager",
                                Department = "Operations",
                                Email = "ivy.adams@example.com",
                                Phone = "5557890123",
                                LinkedInUrl = "https://www.linkedin.com/in/ivyadams",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 12,
                                FirstName = "Jack",
                                LastName = "Scott",
                                Title = "CEO",
                                Department = "Executive",
                                Email = "jack.scott@example.com",
                                Phone = "5558901234",
                                LinkedInUrl = "https://www.linkedin.com/in/jackscott",
                                IsVip = true,
                            },
                            new Contact
                            {
                                Id = 13,
                                FirstName = "Kathy",
                                LastName = "Elizabeth",
                                Title = "HR Specialist",
                                Department = "Human Resources",
                                Email = "kathy.morris@example.com",
                                Phone = "5559012345",
                                LinkedInUrl = "https://www.linkedin.com/in/kathymorris",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 14,
                                FirstName = "Louis",
                                LastName = "Alexandr",
                                Title = "Customer Service Lead",
                                Department = "Customer Service",
                                Email = "louis.walker@example.com",
                                Phone = "5550123456",
                                LinkedInUrl = "https://www.linkedin.com/in/louiswalker",
                                IsVip = true,
                            },
                            new Contact
                            {
                                Id = 15,
                                FirstName = "Mona",
                                LastName = "Grace",
                                Title = "Legal Advisor",
                                Department = "Legal",
                                Email = "mona.white@example.com",
                                Phone = "5551234567",
                                LinkedInUrl = "https://www.linkedin.com/in/monawhite",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 16,
                                FirstName = "James",
                                LastName = "T.",
                                Title = "Marketing Manager",
                                Department = "Marketing",
                                Email = "james.smith@example.com",
                                Phone = "5559876543",
                                LinkedInUrl = "https://www.linkedin.com/in/jamessmith",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 17,
                                FirstName = "Sarah",
                                LastName = "A.",
                                Title = "HR Specialist",
                                Department = "Human Resources",
                                Email = "sarah.johnson@example.com",
                                Phone = "5551122334",
                                LinkedInUrl = "https://www.linkedin.com/in/sarahjohnson",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 18,
                                FirstName = "DavId",
                                LastName = "L.",
                                Title = "Chief Executive Officer",
                                Department = "Executive",
                                Email = "davId.brown@example.com",
                                Phone = "5552233445",
                                LinkedInUrl = "https://www.linkedin.com/in/davIdbrown",
                                IsVip = true,
                            },
                            new Contact
                            {
                                Id = 19,
                                FirstName = "Emily",
                                LastName = "M.",
                                Title = "Product Designer",
                                Department = "Design",
                                Email = "emily.williams@example.com",
                                Phone = "5556677889",
                                LinkedInUrl = "https://www.linkedin.com/in/emilywilliams",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 20,
                                FirstName = "Michael",
                                LastName = "J.",
                                Title = "Sales Director",
                                Department = "Sales",
                                Email = "michael.davis@example.com",
                                Phone = "5558899001",
                                LinkedInUrl = "https://www.linkedin.com/in/michaeldavis",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 21,
                                FirstName = "Olivia",
                                LastName = "K.",
                                Title = "Chief Financial Officer",
                                Department = "Finance",
                                Email = "olivia.martinez@example.com",
                                Phone = "5553456789",
                                LinkedInUrl = "https://www.linkedin.com/in/oliviamartinez",
                                IsVip = true,
                            },
                            new Contact
                            {
                                Id = 22,
                                FirstName = "Ethan",
                                LastName = "B.",
                                Title = "IT Manager",
                                Department = "IT",
                                Email = "ethan.taylor@example.com",
                                Phone = "5552345678",
                                LinkedInUrl = "https://www.linkedin.com/in/ethantaylor",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 23,
                                FirstName = "Sophia",
                                LastName = "J.",
                                Title = "Operations Coordinator",
                                Department = "Operations",
                                Email = "sophia.wilson@example.com",
                                Phone = "5556781234",
                                LinkedInUrl = "https://www.linkedin.com/in/sophiawilson",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 24,
                                FirstName = "Daniel",
                                LastName = "P.",
                                Title = "Customer Success Manager",
                                Department = "Customer Support",
                                Email = "daniel.moore@example.com",
                                Phone = "5559988776",
                                LinkedInUrl = "https://www.linkedin.com/in/danielmoore",
                                IsVip = false,
                            },
                        new Contact
                        {
                            Id = 25,
                            FirstName = "Chloe",
                            LastName = "S.",
                            Title = "Senior Analyst",
                            Department = "Finance",
                            Email = "chloe.martin@example.com",
                            Phone = "5557766554",
                            LinkedInUrl = "https://www.linkedin.com/in/chloemartin",
                            IsVip = true,
                        },
                        new Contact
                        {
                            Id = 26,
                            FirstName = "Liam",
                            LastName = "G.",
                            Title = "Project Manager",
                            Department = "Operations",
                            Email = "liam.green@example.com",
                            Phone = "5554433221",
                            LinkedInUrl = "https://www.linkedin.com/in/liamgreen",
                            IsVip = false,
                        },
                        new Contact
                        {
                            Id = 27,
                            FirstName = "Isabella",
                            LastName = "H.",
                            Title = "Marketing Director",
                            Department = "Marketing",
                            Email = "isabella.hudson@example.com",
                            Phone = "5559988776",
                            LinkedInUrl = "https://www.linkedin.com/in/isabellahudson",
                            IsVip = true,
                        },
                        new Contact
                        {
                            Id = 28,
                            FirstName = "Ethan",
                            LastName = "P.",
                            Title = "Sales Manager",
                            Department = "Sales",
                            Email = "ethan.peters@example.com",
                            Phone = "5551122334",
                            LinkedInUrl = "https://www.linkedin.com/in/ethanpeters",
                            IsVip = false,
                        },
                        new Contact
                        {
                            Id = 29,
                            FirstName = "Ava",
                            LastName = "W.",
                            Title = "Human Resources Specialist",
                            Department = "HR",
                            Email = "ava.williams@example.com",
                            Phone = "5556677889",
                            LinkedInUrl = "https://www.linkedin.com/in/avawilliams",
                            IsVip = true,
                        },
                        new Contact
                        {
                            Id = 30,
                            FirstName = "Mason",
                            LastName = "J.",
                            Title = "Data Scientist",
                            Department = "IT",
                            Email = "mason.james@example.com",
                            Phone = "5554455667",
                            LinkedInUrl = "https://www.linkedin.com/in/masonjames",
                            IsVip = false,
                        },
                        new Contact
                        {
                            Id = 31,
                            FirstName = "Sophia",
                            LastName = "K.",
                            Title = "Customer Support Lead",
                            Department = "Customer Service",
                            Email = "sophia.king@example.com",
                            Phone = "5552334455",
                            LinkedInUrl = "https://www.linkedin.com/in/sophiaking",
                            IsVip = true,
                        },
                        new Contact
                        {
                            Id = 32,
                            FirstName = "Jackson",
                            LastName = "T.",
                            Title = "Chief Technology Officer",
                            Department = "Technology",
                            Email = "jackson.taylor@example.com",
                            Phone = "5555555555",
                            LinkedInUrl = "https://www.linkedin.com/in/jacksontaylor",
                            IsVip = true,
                        },
                        new Contact
                        {
                            Id = 33,
                            FirstName = "Charlotte",
                            LastName = "L.",
                            Title = "Finance Manager",
                            Department = "Finance",
                            Email = "charlotte.larson@example.com",
                            Phone = "5552233446",
                            LinkedInUrl = "https://www.linkedin.com/in/charlottelarson",
                            IsVip = false,
                        },
                        new Contact
                        {
                            Id = 34,
                            FirstName = "Lucas",
                            LastName = "B.",
                            Title = "IT Specialist",
                            Department = "IT",
                            Email = "lucas.brown@example.com",
                            Phone = "5556677880",
                            LinkedInUrl = "https://www.linkedin.com/in/lucasbrown",
                            IsVip = true,
                        },
                        new Contact
                        {
                            Id = 35,
                            FirstName = "Mia",
                            LastName = "C.",
                            Title = "Legal Advisor",
                            Department = "Legal",
                            Email = "mia.carter@example.com",
                            Phone = "5553334446",
                            LinkedInUrl = "https://www.linkedin.com/in/miacarter",
                            IsVip = true,
                        },
                        new Contact
                        {
                            Id = 36,
                            FirstName = "Logan",
                            LastName = "D.",
                            Title = "Operations Manager",
                            Department = "Operations",
                            Email = "logan.davis@example.com",
                            Phone = "5553344556",
                            LinkedInUrl = "https://www.linkedin.com/in/logandavis",
                            IsVip = false,
                        },
                        new Contact
                        {
                            Id = 37,
                            FirstName = "Harper",
                            LastName = "M.",
                            Title = "Product Manager",
                            Department = "Product",
                            Email = "harper.morris@example.com",
                            Phone = "5555566778",
                            LinkedInUrl = "https://www.linkedin.com/in/harpermorris",
                            IsVip = true,
                        },
                        new Contact
                        {
                            Id = 38,
                            FirstName = "Benjamin",
                            LastName = "N.",
                            Title = "Chief Executive Officer",
                            Department = "Executive",
                            Email = "benjamin.nelson@example.com",
                            Phone = "5551223344",
                            LinkedInUrl = "https://www.linkedin.com/in/benjaminnelson",
                            IsVip = true,
                        },
                        new Contact
                        {
                            Id = 39,
                            FirstName = "Ella",
                            LastName = "O.",
                            Title = "Public Relations Manager",
                            Department = "PR",
                            Email = "ella.olson@example.com",
                            Phone = "5554455668",
                            LinkedInUrl = "https://www.linkedin.com/in/ellaolson",
                            IsVip = false,
                        },
                        new Contact
                        {
                            Id = 40,
                            FirstName = "James",
                            LastName = "P.",
                            Title = "Chief Marketing Officer",
                            Department = "Marketing",
                            Email = "james.phillips@example.com",
                            Phone = "5557768999",
                            LinkedInUrl = "https://www.linkedin.com/in/jamesphillips",
                            IsVip = true,
                        },
                        new Contact
                        {
                            Id = 41,
                            FirstName = "Astarion",
                            LastName = "Ancunin",
                            Title = "Manager",
                            Department = "Sales",
                            Email = "iplayedbg3toomuch@example.com",
                            Phone = "7077077777",
                            LinkedInUrl = "https://www.linkedin.com/in/johndoe",
                            IsVip = false,
                        },
                        new Contact
                        {
                            Id = 42,
                            FirstName = "Johnny",
                            LastName = "Thomas",
                            Title = "Manager",
                            Department = "Sales",
                            Email = "john.doe@example.com",
                            Phone = "1234567890",
                            LinkedInUrl = "https://www.linkedin.com/in/johndoe",
                            IsVip = true,
                        },

                            new Contact
                            {
                                Id = 43,
                                FirstName = "Jannie",
                                LastName = "Smith",
                                Title = "Director",
                                Department = "Marketing",
                                Email = "jane.smith@example.com",
                                Phone = "9876543210",
                                LinkedInUrl = "https://www.linkedin.com/in/janesmith",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 44,
                                FirstName = "Ali",
                                LastName = "Johnson",
                                Title = "VP",
                                Department = "Human Resources",
                                Email = "alice.johnson@example.com",
                                Phone = "5551234567",
                                LinkedInUrl = "https://www.linkedin.com/in/alicejohnson",
                                IsVip = true,
                            },
                            new Contact
                            {
                                Id = 45,
                                FirstName = "Bobby",
                                LastName = "Joe",
                                Title = "Chief Financial Officer",
                                Department = "Finance",
                                Email = "bob.brown@example.com",
                                Phone = "5557654321",
                                LinkedInUrl = "https://www.linkedin.com/in/bobbrown",
                                IsVip = true,
                            },
                            new Contact
                            {
                                Id = 46,
                                FirstName = "C",
                                LastName = "Davis",
                                Title = "Chief Operating Officer",
                                Department = "Operations",
                                Email = "charlie.davis@example.com",
                                Phone = "5557890123",
                                LinkedInUrl = "https://www.linkedin.com/in/charliedavis",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 47,
                                FirstName = "D",
                                LastName = "Williams",
                                Title = "Director of Technology",
                                Department = "Technology",
                                Email = "deborah.williams@example.com",
                                Phone = "5552345678",
                                LinkedInUrl = "https://www.linkedin.com/in/deborahwilliams",
                                IsVip = true,
                            },
                            new Contact
                            {
                                Id = 48,
                                FirstName = "E",
                                LastName = "Marie",
                                Title = "Marketing Specialist",
                                Department = "Marketing",
                                Email = "eve.taylor@example.com",
                                Phone = "5553456789",
                                LinkedInUrl = "https://www.linkedin.com/in/evetaylor",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 49,
                                FirstName = "F",
                                LastName = "Harris",
                                Title = "Senior Engineer",
                                Department = "Engineering",
                                Email = "frank.harris@example.com",
                                Phone = "5554567890",
                                LinkedInUrl = "https://www.linkedin.com/in/frankharris",
                                IsVip = true,
                            },
                            new Contact
                            {
                                Id = 50,
                                FirstName = "G",
                                LastName = "King",
                                Title = "Business Development Manager",
                                Department = "Sales",
                                Email = "grace.king@example.com",
                                Phone = "5555678901",
                                LinkedInUrl = "https://www.linkedin.com/in/graceking",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 51,
                                FirstName = "H",
                                LastName = "Lee",
                                Title = "Head of Research",
                                Department = "Research and Development",
                                Email = "hank.lee@example.com",
                                Phone = "5556789012",
                                LinkedInUrl = "https://www.linkedin.com/in/hanklee",
                                IsVip = true,
                            },
                            new Contact
                            {
                                Id = 52,
                                FirstName = "I",
                                LastName = "Adams",
                                Title = "Project Manager",
                                Department = "Operations",
                                Email = "ivy.adams@example.com",
                                Phone = "5557890123",
                                LinkedInUrl = "https://www.linkedin.com/in/ivyadams",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 53,
                                FirstName = "Jackson",
                                LastName = "Scott",
                                Title = "CEO",
                                Department = "Executive",
                                Email = "jack.scott@example.com",
                                Phone = "5558901234",
                                LinkedInUrl = "https://www.linkedin.com/in/jackscott",
                                IsVip = true,
                            },
                            new Contact
                            {
                                Id = 54,
                                FirstName = "K",
                                LastName = "Elizabeth",
                                Title = "HR Specialist",
                                Department = "Human Resources",
                                Email = "kathy.morris@example.com",
                                Phone = "5559012345",
                                LinkedInUrl = "https://www.linkedin.com/in/kathymorris",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 55,
                                FirstName = "L",
                                LastName = "Alexandr",
                                Title = "Customer Service Lead",
                                Department = "Customer Service",
                                Email = "louis.walker@example.com",
                                Phone = "5550123456",
                                LinkedInUrl = "https://www.linkedin.com/in/louiswalker",
                                IsVip = true,
                            },
                            new Contact
                            {
                                Id = 56,
                                FirstName = "M",
                                LastName = "Grace",
                                Title = "Legal Advisor",
                                Department = "Legal",
                                Email = "mona.white@example.com",
                                Phone = "5551234567",
                                LinkedInUrl = "https://www.linkedin.com/in/monawhite",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 57,
                                FirstName = "Jameson",
                                LastName = "T.",
                                Title = "Marketing Manager",
                                Department = "Marketing",
                                Email = "james.smith@example.com",
                                Phone = "5559876543",
                                LinkedInUrl = "https://www.linkedin.com/in/jamessmith",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 58,
                                FirstName = "S",
                                LastName = "A.",
                                Title = "HR Specialist",
                                Department = "Human Resources",
                                Email = "sarah.johnson@example.com",
                                Phone = "5551122334",
                                LinkedInUrl = "https://www.linkedin.com/in/sarahjohnson",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 59,
                                FirstName = "Davis",
                                LastName = "L.",
                                Title = "Chief Executive Officer",
                                Department = "Executive",
                                Email = "davId.brown@example.com",
                                Phone = "5552233445",
                                LinkedInUrl = "https://www.linkedin.com/in/davIdbrown",
                                IsVip = true,
                            },
                            new Contact
                            {
                                Id = 60,
                                FirstName = "Easter",
                                LastName = "M.",
                                Title = "Product Designer",
                                Department = "Design",
                                Email = "emily.williams@example.com",
                                Phone = "5556677889",
                                LinkedInUrl = "https://www.linkedin.com/in/emilywilliams",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 61,
                                FirstName = "Miguel",
                                LastName = "J.",
                                Title = "Sales Director",
                                Department = "Sales",
                                Email = "michael.davis@example.com",
                                Phone = "5558899001",
                                LinkedInUrl = "https://www.linkedin.com/in/michaeldavis",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 62,
                                FirstName = "Olivera",
                                LastName = "K.",
                                Title = "Chief Financial Officer",
                                Department = "Finance",
                                Email = "olivia.martinez@example.com",
                                Phone = "5553456789",
                                LinkedInUrl = "https://www.linkedin.com/in/oliviamartinez",
                                IsVip = true,
                            },
                            new Contact
                            {
                                Id = 63,
                                FirstName = "Eshy",
                                LastName = "B.",
                                Title = "IT Manager",
                                Department = "IT",
                                Email = "ethan.taylor@example.com",
                                Phone = "5552345678",
                                LinkedInUrl = "https://www.linkedin.com/in/ethantaylor",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 64,
                                FirstName = "Sydney",
                                LastName = "J.",
                                Title = "Operations Coordinator",
                                Department = "Operations",
                                Email = "sophia.wilson@example.com",
                                Phone = "5556781234",
                                LinkedInUrl = "https://www.linkedin.com/in/sophiawilson",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 65,
                                FirstName = "Droo",
                                LastName = "P.",
                                Title = "Customer Success Manager",
                                Department = "Customer Support",
                                Email = "daniel.moore@example.com",
                                Phone = "5559988776",
                                LinkedInUrl = "https://www.linkedin.com/in/danielmoore",
                                IsVip = false,
                            },
                        new Contact
                        {
                            Id = 66,
                            FirstName = "Camber",
                            LastName = "S.",
                            Title = "Senior Analyst",
                            Department = "Finance",
                            Email = "chloe.martin@example.com",
                            Phone = "5557766554",
                            LinkedInUrl = "https://www.linkedin.com/in/chloemartin",
                            IsVip = true,
                        },
                        new Contact
                        {
                            Id = 67,
                            FirstName = "Libero",
                            LastName = "G.",
                            Title = "Project Manager",
                            Department = "Operations",
                            Email = "liam.green@example.com",
                            Phone = "5554433221",
                            LinkedInUrl = "https://www.linkedin.com/in/liamgreen",
                            IsVip = false,
                        },
                        new Contact
                        {
                            Id = 68,
                            FirstName = "Ishy",
                            LastName = "H.",
                            Title = "Marketing Director",
                            Department = "Marketing",
                            Email = "isabella.hudson@example.com",
                            Phone = "5559988776",
                            LinkedInUrl = "https://www.linkedin.com/in/isabellahudson",
                            IsVip = true,
                        },
                        new Contact
                        {
                            Id = 69,
                            FirstName = "Estan",
                            LastName = "P.",
                            Title = "Sales Manager",
                            Department = "Sales",
                            Email = "ethan.peters@example.com",
                            Phone = "5551122334",
                            LinkedInUrl = "https://www.linkedin.com/in/ethanpeters",
                            IsVip = false,
                        },
                        new Contact
                        {
                            Id = 70,
                            FirstName = "Avvy",
                            LastName = "W.",
                            Title = "Human Resources Specialist",
                            Department = "HR",
                            Email = "ava.williams@example.com",
                            Phone = "5556677889",
                            LinkedInUrl = "https://www.linkedin.com/in/avawilliams",
                            IsVip = true,
                        },
                        new Contact
                        {
                            Id = 71,
                            FirstName = "Masoon",
                            LastName = "J.",
                            Title = "Data Scientist",
                            Department = "IT",
                            Email = "mason.james@example.com",
                            Phone = "5554455667",
                            LinkedInUrl = "https://www.linkedin.com/in/masonjames",
                            IsVip = false,
                        },
                        new Contact
                        {
                            Id = 72,
                            FirstName = "Sopher",
                            LastName = "K.",
                            Title = "Customer Support Lead",
                            Department = "Customer Service",
                            Email = "sophia.king@example.com",
                            Phone = "5552334455",
                            LinkedInUrl = "https://www.linkedin.com/in/sophiaking",
                            IsVip = true,
                        },
                        new Contact
                        {
                            Id = 73,
                            FirstName = "Jacky",
                            LastName = "T.",
                            Title = "Chief Technology Officer",
                            Department = "Technology",
                            Email = "jackson.taylor@example.com",
                            Phone = "5555555555",
                            LinkedInUrl = "https://www.linkedin.com/in/jacksontaylor",
                            IsVip = true,
                        },
                        new Contact
                        {
                            Id = 74,
                            FirstName = "Char",
                            LastName = "L.",
                            Title = "Finance Manager",
                            Department = "Finance",
                            Email = "charlotte.larson@example.com",
                            Phone = "5552233446",
                            LinkedInUrl = "https://www.linkedin.com/in/charlottelarson",
                            IsVip = false,
                        },
                        new Contact
                        {
                            Id = 75,
                            FirstName = "Lucky",
                            LastName = "B.",
                            Title = "IT Specialist",
                            Department = "IT",
                            Email = "lucas.brown@example.com",
                            Phone = "5556677880",
                            LinkedInUrl = "https://www.linkedin.com/in/lucasbrown",
                            IsVip = true,
                        },
                        new Contact
                        {
                            Id = 76,
                            FirstName = "Mustafa",
                            LastName = "C.",
                            Title = "Legal Advisor",
                            Department = "Legal",
                            Email = "mia.carter@example.com",
                            Phone = "5553334446",
                            LinkedInUrl = "https://www.linkedin.com/in/miacarter",
                            IsVip = true,
                        },
                        new Contact
                        {
                            Id = 77,
                            FirstName = "Lvvy",
                            LastName = "D.",
                            Title = "Operations Manager",
                            Department = "Operations",
                            Email = "logan.davis@example.com",
                            Phone = "5553344556",
                            LinkedInUrl = "https://www.linkedin.com/in/logandavis",
                            IsVip = false,
                        },
                        new Contact
                        {
                            Id = 78,
                            FirstName = "Hopper",
                            LastName = "M.",
                            Title = "Product Manager",
                            Department = "Product",
                            Email = "harper.morris@example.com",
                            Phone = "5555566778",
                            LinkedInUrl = "https://www.linkedin.com/in/harpermorris",
                            IsVip = true,
                        },
                        new Contact
                        {
                            Id = 79,
                            FirstName = "Benny",
                            LastName = "N.",
                            Title = "Chief Executive Officer",
                            Department = "Executive",
                            Email = "benjamin.nelson@example.com",
                            Phone = "5551223344",
                            LinkedInUrl = "https://www.linkedin.com/in/benjaminnelson",
                            IsVip = true,
                        },
                        new Contact
                        {
                            Id = 80,
                            FirstName = "Elvish",
                            LastName = "O.",
                            Title = "Public Relations Manager",
                            Department = "PR",
                            Email = "ella.olson@example.com",
                            Phone = "5554455668",
                            LinkedInUrl = "https://www.linkedin.com/in/ellaolson",
                            IsVip = false,
                        },
                        new Contact
                        {
                            Id = 81,
                            FirstName = "Jose",
                            LastName = "P.",
                            Title = "Chief Marketing Officer",
                            Department = "Marketing",
                            Email = "james.phillips@example.com",
                            Phone = "5557768999",
                            LinkedInUrl = "https://www.linkedin.com/in/jamesphillips",
                            IsVip = true,
                        },
                        new Contact
                        {
                            Id = 82,
                            FirstName = "J",
                            LastName = "Thomas",
                            Title = "Manager",
                            Department = "Sales",
                            Email = "john.doe@example.com",
                            Phone = "1234567890",
                            LinkedInUrl = "https://www.linkedin.com/in/johndoe",
                            IsVip = true,
                        },

                            new Contact
                            {
                                Id = 83,
                                FirstName = "June",
                                LastName = "Smith",
                                Title = "Director",
                                Department = "Marketing",
                                Email = "jane.smith@example.com",
                                Phone = "9876543210",
                                LinkedInUrl = "https://www.linkedin.com/in/janesmith",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 84,
                                FirstName = "Almon",
                                LastName = "Johnson",
                                Title = "VP",
                                Department = "Human Resources",
                                Email = "alice.johnson@example.com",
                                Phone = "5551234567",
                                LinkedInUrl = "https://www.linkedin.com/in/alicejohnson",
                                IsVip = true,
                            },
                            new Contact
                            {
                                Id = 85,
                                FirstName = "Boston",
                                LastName = "Joe",
                                Title = "Chief Financial Officer",
                                Department = "Finance",
                                Email = "bob.brown@example.com",
                                Phone = "5557654321",
                                LinkedInUrl = "https://www.linkedin.com/in/bobbrown",
                                IsVip = true,
                            },
                            new Contact
                            {
                                Id = 86,
                                FirstName = "Cristin",
                                LastName = "Davis",
                                Title = "Chief Operating Officer",
                                Department = "Operations",
                                Email = "charlie.davis@example.com",
                                Phone = "5557890123",
                                LinkedInUrl = "https://www.linkedin.com/in/charliedavis",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 87,
                                FirstName = "Debb",
                                LastName = "Williams",
                                Title = "Director of Technology",
                                Department = "Technology",
                                Email = "deborah.williams@example.com",
                                Phone = "5552345678",
                                LinkedInUrl = "https://www.linkedin.com/in/deborahwilliams",
                                IsVip = true,
                            },
                            new Contact
                            {
                                Id = 88,
                                FirstName = "Enger",
                                LastName = "Marie",
                                Title = "Marketing Specialist",
                                Department = "Marketing",
                                Email = "eve.taylor@example.com",
                                Phone = "5553456789",
                                LinkedInUrl = "https://www.linkedin.com/in/evetaylor",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 89,
                                FirstName = "Frankfurt",
                                LastName = "Harris",
                                Title = "Senior Engineer",
                                Department = "Engineering",
                                Email = "frank.harris@example.com",
                                Phone = "5554567890",
                                LinkedInUrl = "https://www.linkedin.com/in/frankharris",
                                IsVip = true,
                            },
                            new Contact
                            {
                                Id = 90,
                                FirstName = "Gracito",
                                LastName = "King",
                                Title = "Business Development Manager",
                                Department = "Sales",
                                Email = "grace.king@example.com",
                                Phone = "5555678901",
                                LinkedInUrl = "https://www.linkedin.com/in/graceking",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 91,
                                FirstName = "Hanker",
                                LastName = "Lee",
                                Title = "Head of Research",
                                Department = "Research and Development",
                                Email = "hank.lee@example.com",
                                Phone = "5556789012",
                                LinkedInUrl = "https://www.linkedin.com/in/hanklee",
                                IsVip = true,
                            },
                            new Contact
                            {
                                Id = 92,
                                FirstName = "Iccy",
                                LastName = "Adams",
                                Title = "Project Manager",
                                Department = "Operations",
                                Email = "ivy.adams@example.com",
                                Phone = "5557890123",
                                LinkedInUrl = "https://www.linkedin.com/in/ivyadams",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 93,
                                FirstName = "Jaam",
                                LastName = "Scott",
                                Title = "CEO",
                                Department = "Executive",
                                Email = "jack.scott@example.com",
                                Phone = "5558901234",
                                LinkedInUrl = "https://www.linkedin.com/in/jackscott",
                                IsVip = true,
                            },
                            new Contact
                            {
                                Id = 94,
                                FirstName = "Kathie",
                                LastName = "Elizabeth",
                                Title = "HR Specialist",
                                Department = "Human Resources",
                                Email = "kathy.morris@example.com",
                                Phone = "5559012345",
                                LinkedInUrl = "https://www.linkedin.com/in/kathymorris",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 95,
                                FirstName = "Loui",
                                LastName = "Alexandr",
                                Title = "Customer Service Lead",
                                Department = "Customer Service",
                                Email = "louis.walker@example.com",
                                Phone = "5550123456",
                                LinkedInUrl = "https://www.linkedin.com/in/louiswalker",
                                IsVip = true,
                            },
                            new Contact
                            {
                                Id = 96,
                                FirstName = "Mon",
                                LastName = "Grace",
                                Title = "Legal Advisor",
                                Department = "Legal",
                                Email = "mona.white@example.com",
                                Phone = "5551234567",
                                LinkedInUrl = "https://www.linkedin.com/in/monawhite",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 97,
                                FirstName = "Jag",
                                LastName = "T.",
                                Title = "Marketing Manager",
                                Department = "Marketing",
                                Email = "james.smith@example.com",
                                Phone = "5559876543",
                                LinkedInUrl = "https://www.linkedin.com/in/jamessmith",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 98,
                                FirstName = "Sar",
                                LastName = "A.",
                                Title = "HR Specialist",
                                Department = "Human Resources",
                                Email = "sarah.johnson@example.com",
                                Phone = "5551122334",
                                LinkedInUrl = "https://www.linkedin.com/in/sarahjohnson",
                                IsVip = false,
                            },
                            new Contact
                            {
                                Id = 99,
                                FirstName = "Daal",
                                LastName = "L.",
                                Title = "Chief Executive Officer",
                                Department = "Executive",
                                Email = "davId.brown@example.com",
                                Phone = "5552233445",
                                LinkedInUrl = "https://www.linkedin.com/in/davIdbrown",
                                IsVip = true,
                            },
                            new Contact
                            {
                                Id = 100,
                                FirstName = "Roti",
                                LastName = "M.",
                                Title = "Product Designer",
                                Department = "Design",
                                Email = "emily.williams@example.com",
                                Phone = "5556677889",
                                LinkedInUrl = "https://www.linkedin.com/in/emilywilliams",
                                IsVip = false,
                            }
                        );
                        context.SaveChanges();

                    }

                    if (!context.MemberContacts.Any())
                    {
                        var memberContacts = new List<MemberContact>();
                        var memberIds = context.Members.Select(m => m.ID).ToList();
                        var contactIds = context.Contacts.Select(c => c.Id).ToList();
                        var random = new Random();

                        foreach (var memberId in memberIds)
                        {
                            // Pick 2–3 random contacts for each member
                            var selectedContacts = contactIds.OrderBy(x => random.Next()).Take(3).ToList();

                            foreach (var contactId in selectedContacts)
                            {
                                // Avoid duplicates
                                if (!memberContacts.Any(mc => mc.MemberId == memberId && mc.ContactId == contactId))
                                {
                                    memberContacts.Add(new MemberContact
                                    {
                                        MemberId = memberId,
                                        ContactId = contactId
                                    });
                                }
                            }
                        }

                        context.MemberContacts.AddRange(memberContacts);
                        context.SaveChanges();
                    }



                    if (!context.Cancellations.Any())
                    {
                        context.Cancellations.AddRange(
                            // 2 Canceled records
                            new Cancellation
                            {
                                CancellationDate = new DateTime(2025, 1, 5),
                                IsCancelled = true,
                                CancellationNote = "Member requested cancellation due to personal reasons.",
                                MemberID = 1  // Assuming Member with Id 1 exists
                            },
                            new Cancellation
                            {
                                CancellationDate = new DateTime(2025, 1, 15),
                                IsCancelled = true,
                                CancellationNote = "Member canceled their subscription after failing to make payments.",
                                MemberID = 2  // Assuming Member with Id 2 exists
                            },

                            // 13 Non-Canceled records
                            new Cancellation
                            {
                                CancellationDate = new DateTime(2025, 1, 10),
                                IsCancelled = true,
                                CancellationNote = "Subscription cancelled.",
                                MemberID = 3
                            },
                            new Cancellation
                            {
                                CancellationDate = new DateTime(2025, 1, 12),
                                IsCancelled = true,
                                CancellationNote = "Member requested cancellation due to personal reasons.",
                                MemberID = 4
                            },
                            new Cancellation
                            {
                                CancellationDate = new DateTime(2025, 1, 18),
                                IsCancelled = true,
                                CancellationNote = "Payment not updated.",
                                MemberID = 5
                            },
                            new Cancellation
                            {
                                CancellationDate = new DateTime(2025, 1, 20),
                                IsCancelled = true,
                                CancellationNote = "Requested cancellation.",
                                MemberID = 6
                            },
                            new Cancellation
                            {
                                CancellationDate = new DateTime(2025, 1, 22),
                                IsCancelled = true,
                                CancellationNote = "Subscription Bundling Issues.",
                                MemberID = 7
                            },
                            new Cancellation
                            {
                                CancellationDate = new DateTime(2025, 1, 25),
                                IsCancelled = true,
                                CancellationNote = "Account Suspension Due to Missed Payments.",
                                MemberID = 8
                            },
                            new Cancellation
                            {
                                CancellationDate = new DateTime(2025, 1, 27),
                                IsCancelled = true,
                                CancellationNote = "Inconsistent Billing Cycle.",
                                MemberID = 9
                            },
                            new Cancellation
                            {
                                CancellationDate = new DateTime(2025, 1, 30),
                                IsCancelled = true,
                                CancellationNote = "Payment Processing Issues.",
                                MemberID = 10
                            },
                            new Cancellation
                            {
                                CancellationDate = new DateTime(2025, 2, 1),
                                IsCancelled = true,
                                CancellationNote = "Subscription Auto-Renewed Without Notice.",
                                MemberID = 11
                            },
                            new Cancellation
                            {
                                CancellationDate = new DateTime(2025, 2, 5),
                                IsCancelled = true,
                                CancellationNote = "Duplicate Charges for Multiple Accounts.",
                                MemberID = 12
                            },
                            new Cancellation
                            {
                                CancellationDate = new DateTime(2025, 2, 10),
                                IsCancelled = true,
                                CancellationNote = "Membership Price Increased.",
                                MemberID = 13
                            },
                            new Cancellation
                            {
                                CancellationDate = new DateTime(2025, 2, 15),
                                IsCancelled = true,
                                CancellationNote = "Bank Placed a Hold on Payments.",
                                MemberID = 14
                            },
                            new Cancellation
                            {
                                CancellationDate = new DateTime(2025, 2, 18),
                                IsCancelled = true,
                                CancellationNote = "Billing Disputes Unresolved.",
                                MemberID = 15
                            }
                        );
                        context.SaveChanges();
                    }
                    //Seed data needed for production and during development

                    if (!context.NAICSCodes.Any())
                    {
                        context.NAICSCodes.AddRange(
                            new NAICSCode { Id = 1, Code = "1111", Description = "Oilseed and Grain Farming" },
                            new NAICSCode { Id = 2, Code = "2111", Description = "Oil and Gas Extraction" },
                            new NAICSCode { Id = 3, Code = "2211", Description = "Electric Power Generation, Transmission, and Distribution" },
                            new NAICSCode { Id = 4, Code = "2361", Description = "Residential Building Construction" },
                            new NAICSCode { Id = 5, Code = "3111", Description = "Animal Food Manufacturing" },
                            new NAICSCode { Id = 6, Code = "4231", Description = "Motor Vehicle and Motor Vehicle Parts and Supplies Wholesalers" },
                            new NAICSCode { Id = 7, Code = "4411", Description = "Automobile Dealers" },
                            new NAICSCode { Id = 8, Code = "4811", Description = "Scheduled Air Transportation" },
                            new NAICSCode { Id = 9, Code = "5111", Description = "Newspaper, Periodical, Book, and Directory Publishers" },
                            new NAICSCode { Id = 10, Code = "5221", Description = "Depository Credit Intermediation" },
                            new NAICSCode { Id = 11, Code = "5311", Description = "Lessors of Real Estate" },
                            new NAICSCode { Id = 12, Code = "5411", Description = "Legal Services" },
                            new NAICSCode { Id = 13, Code = "5611", Description = "Office Administrative Services" },
                            new NAICSCode { Id = 14, Code = "6111", Description = "Elementary and Secondary Schools" },
                            new NAICSCode { Id = 15, Code = "6211", Description = "Offices of Physicians" },
                            new NAICSCode { Id = 16, Code = "7111", Description = "Performing Arts Companies" },
                            new NAICSCode { Id = 17, Code = "7211", Description = "Traveler Accommodation" },
                            new NAICSCode { Id = 18, Code = "8111", Description = "Automotive Repair and Maintenance" },
                            new NAICSCode { Id = 19, Code = "9211", Description = "Executive Offices" }
                        );

                        // Save changes to persist the data
                        context.SaveChanges();
                    }

                    if (!context.IndustryNAICSCodes.Any())
                    {
                        var industryNAICSCodes = new List<IndustryNAICSCode>();
                        var random = new Random();
                        int maxMembers = context.Members.Count();

                        for (int i = 1; i <= 100; i++) // Create 100 IndustryNAICSCode records
                        {
                            industryNAICSCodes.Add(new IndustryNAICSCode
                            {
                                Id = i,
                                MemberId = (i <= maxMembers) ? i : random.Next(1, maxMembers + 1), // Ensures valid MemberId
                                NAICSCodeId = random.Next(1, 20) // Randomly selects a NAICSCodeId between 1 and 19
                            });
                        }

                        context.IndustryNAICSCodes.AddRange(industryNAICSCodes);
                        context.SaveChanges();
                    }



                    if (!context.ProductionEmails.Any())
                    {
                        // Random member names to be used in the Email content
                        var randomNames = new List<string>
                            {
                                "John Doe", "Jane Smith", "Robert Johnson", "Emily Davis", "Michael Brown"
                            };

                        // Seeding predefined production Email templates with random member names
                        context.ProductionEmails.AddRange(
                             new ProductionEmail
                             {
                                 Id = 1,
                                 TemplateName = "Primary Welcome",
                                 EmailType = EmailType.Welcome,
                                 Subject = "Welcome to NIA!",
                                 Body = $"Welcome to the NIA community! We are thrilled to have you onboard. Please let us know if you need any assistance.\n\nBest regards,\nNIA Team",
                                 //IsGood = true
                             },
                             new ProductionEmail
                             {
                                 Id = 2,
                                 TemplateName = "Subscription Reminder",
                                 EmailType = EmailType.Reminder,
                                 Subject = "Membership Renewal Reminder",
                                 Body = $"Dear {randomNames[1]},\n\nYour membership with NIA is about to expire on 2025-02-15. Please renew your membership to continue enjoying all the benefits.\n\nBest regards,\nNIA Team",
                                 //IsGood = true
                             },
                             new ProductionEmail
                             {
                                 Id = 3,
                                 TemplateName = "Cancellation Clarify",
                                 EmailType = EmailType.Cancellation,
                                 Subject = "Membership Cancellation Confirmation",
                                 Body = $"Dear {randomNames[2]},\n\nWe are sorry to see you go. Your membership has been successfully canceled. If you change your mind in the future, we’d love to have you back.\n\nBest regards,\nNIA Team",
                                 //IsGood = true
                             },
                             new ProductionEmail
                             {
                                 Id = 4,
                                 TemplateName = "Membership Update",
                                 EmailType = EmailType.ProductUpdates,
                                 Subject = "Important Membership Update",
                                 Body = $"Dear {randomNames[3]},\n\nWe would like to inform you of an important update regarding your membership status. Please log in to your account for more details.\n\nBest regards,\nNIA Team",
                                 //IsGood = true
                             },
                             new ProductionEmail
                             {
                                 Id = 5,
                                 TemplateName = "Maintance Update",
                                 EmailType = EmailType.Other,
                                 Subject = "NIA System Update",
                                 Body = $"Dear {randomNames[4]},\n\nThis is a notification regarding a recent update to the NIA system. We encourage you to check out the new features and improvements.\n\nBest regards,\nNIA Team",
                                 //IsGood = true
                             });
                        context.SaveChanges();
                    }

                    if (!context.MTags.Any())
                    {
                        var mTags = new List<MTag>
                        {
                            new MTag { Id = 1, MTagName = "Premium", MTagDescription = "Premium membership tag" },
                            new MTag { Id = 2, MTagName = "New Member", MTagDescription = "Recently joined members" },
                            new MTag { Id = 3, MTagName = "VIP", MTagDescription = "High-priority clients" },
                            new MTag { Id = 4, MTagName = "Enterprise", MTagDescription = "Enterprise-level members" },
                            new MTag { Id = 5, MTagName = "Small Business", MTagDescription = "Small business clients" },
                            new MTag { Id = 6, MTagName = "Non-Profit", MTagDescription = "Non-profit organizations" },
                            new MTag { Id = 7, MTagName = "Gold", MTagDescription = "Gold-tier members" },
                            new MTag { Id = 8, MTagName = "Silver", MTagDescription = "Silver-tier members" },
                            new MTag { Id = 9, MTagName = "Bronze", MTagDescription = "Bronze-tier members" },
                            new MTag { Id = 10, MTagName = "Annual", MTagDescription = "Annual subscription members" },
                            new MTag { Id = 11, MTagName = "Quarterly", MTagDescription = "Quarterly subscription members" },
                            new MTag { Id = 12, MTagName = "Monthly", MTagDescription = "Monthly subscription members" },
                            new MTag { Id = 13, MTagName = "Government", MTagDescription = "Government contracts" },
                            new MTag { Id = 14, MTagName = "Retail", MTagDescription = "Retail clients" },
                            new MTag { Id = 15, MTagName = "Wholesale", MTagDescription = "Wholesale clients" },
                            new MTag { Id = 16, MTagName = "Education", MTagDescription = "Educational institutions" },
                            new MTag { Id = 17, MTagName = "Healthcare", MTagDescription = "Healthcare organizations" },
                            new MTag { Id = 18, MTagName = "Technology", MTagDescription = "Tech industry members" },
                            new MTag { Id = 19, MTagName = "Finance", MTagDescription = "Financial sector clients" },
                            new MTag { Id = 20, MTagName = "Legal", MTagDescription = "Law firms and legal advisors" },
                            new MTag { Id = 21, MTagName = "Real Estate", MTagDescription = "Real estate businesses" },
                            new MTag { Id = 22, MTagName = "Construction", MTagDescription = "Construction companies" },
                            new MTag { Id = 23, MTagName = "Freelancer", MTagDescription = "Independent contractors" },
                            new MTag { Id = 24, MTagName = "Startup", MTagDescription = "New startup businesses" },
                            new MTag { Id = 25, MTagName = "Non-Active", MTagDescription = "Inactive or dormant members" }
                        };

                        context.MTags.AddRange(mTags);
                        context.SaveChanges();
                    }

                    if (!context.MemberTags.Any())
                    {
                        var random = new Random();
                        var memberTags = new List<MemberTag>();

                        int maxMembers = context.Members.Count(); // Ensure we assign tags to valid members

                        for (int i = 1; i <= 25; i++) // Create 25 MemberTag records
                        {
                            memberTags.Add(new MemberTag
                            {
                                Id = i,
                                MemberId = random.Next(1, maxMembers + 1), // Randomly assign to existing members
                                MTagID = random.Next(1, 26) // Assign random tag from 1 to 25
                            });
                        }

                        context.MemberTags.AddRange(memberTags);
                        context.SaveChanges();
                    }

                    if (!context.Sectors.Any())
                    {
                        var sectors = new List<Sector>
                        {
                            new Sector { Id = 1, SectorName = "Technology", SectorDescription = "Tech companies and startups" },
                            new Sector { Id = 2, SectorName = "Healthcare", SectorDescription = "Hospitals and medical services" },
                            new Sector { Id = 3, SectorName = "Finance", SectorDescription = "Banks, insurance, and financial institutions" },
                            new Sector { Id = 4, SectorName = "Retail", SectorDescription = "Retail stores and e-commerce" },
                            new Sector { Id = 5, SectorName = "Education", SectorDescription = "Schools, universities, and training centers" },
                            new Sector { Id = 6, SectorName = "Manufacturing", SectorDescription = "Industrial and production companies" },
                            new Sector { Id = 7, SectorName = "Government", SectorDescription = "Public sector organizations" },
                            new Sector { Id = 8, SectorName = "Energy", SectorDescription = "Oil, gas, and renewable energy companies" },
                            new Sector { Id = 9, SectorName = "Real Estate", SectorDescription = "Property management and real estate firms" },
                            new Sector { Id = 10, SectorName = "Legal", SectorDescription = "Law firms and legal consultants" },
                            new Sector { Id = 11, SectorName = "Hospitality", SectorDescription = "Hotels, restaurants, and tourism" },
                            new Sector { Id = 12, SectorName = "Non-Profit", SectorDescription = "Non-profit organizations and charities" },
                            new Sector { Id = 13, SectorName = "Construction", SectorDescription = "Builders and infrastructure companies" },
                            new Sector { Id = 14, SectorName = "Media", SectorDescription = "Broadcasting, journalism, and digital media" },
                            new Sector { Id = 15, SectorName = "Transportation", SectorDescription = "Logistics, shipping, and travel" },
                            new Sector { Id = 16, SectorName = "Telecommunications", SectorDescription = "Telecom and internet service providers" },
                            new Sector { Id = 17, SectorName = "Agriculture", SectorDescription = "Farming, food production, and agribusiness" },
                            new Sector { Id = 18, SectorName = "Automotive", SectorDescription = "Car manufacturers and dealerships" },
                            new Sector { Id = 19, SectorName = "Entertainment", SectorDescription = "Film, music, and gaming industries" },
                            new Sector { Id = 20, SectorName = "Aerospace", SectorDescription = "Aviation and space exploration companies" },
                            new Sector { Id = 21, SectorName = "Pharmaceuticals", SectorDescription = "Drug manufacturers and biotech firms" },
                            new Sector { Id = 22, SectorName = "Cybersecurity", SectorDescription = "Security firms and ethical hacking services" },
                            new Sector { Id = 23, SectorName = "E-commerce", SectorDescription = "Online marketplaces and digital sales" },
                            new Sector { Id = 24, SectorName = "HR & Recruitment", SectorDescription = "Talent acquisition and HR firms" },
                            new Sector { Id = 25, SectorName = "Sports & Fitness", SectorDescription = "Gyms, sports clubs, and athletic organizations" }
                        };

                        context.Sectors.AddRange(sectors);
                        context.SaveChanges();
                    }

                    if (!context.MemberSectors.Any())
                    {
                        var random = new Random();
                        var memberSectors = new List<MemberSector>();

                        int maxMembers = context.Members.Count(); // Ensure valid member IDs exist

                        for (int i = 1; i <= 25; i++) // Create 25 MemberSector records
                        {
                            memberSectors.Add(new MemberSector
                            {
                                Id = i,
                                MemberId = random.Next(1, maxMembers + 1), // Randomly assign to existing members
                                SectorId = random.Next(1, 26) // Assign random sector from 1 to 25
                            });
                        }

                        context.MemberSectors.AddRange(memberSectors);
                        context.SaveChanges();
                    }

                    if (!context.Opportunities.Any())
                    {
                        context.Opportunities.AddRange(
                            new Opportunity { ID = 1, OpportunityName = "Gym Equipment Upgrade", OpportunityAction = "Follow up with vendor", POC = "John Doe", Account = "ABC Fitness", Interaction = "Email Inquiry", LastContact = DateTime.Parse("2024-01-15"), OpportunityStatus = OpportunityStatus.Qualification, OpportunityPriority = OpportunityPriority.High },
                            new Opportunity { ID = 2, OpportunityName = "New Membership Plan", OpportunityAction = "Schedule demo", POC = "Jane Smith", Account = "XYZ Gym", Interaction = "Phone Call", LastContact = DateTime.Parse("2024-02-10"), OpportunityStatus = OpportunityStatus.Negotiating, OpportunityPriority = OpportunityPriority.ExtremelyHigh },
                            new Opportunity { ID = 3, OpportunityName = "Corporate Wellness Program", OpportunityAction = "Prepare proposal", POC = "Mike Johnson", Account = "FitCorp", Interaction = "Meeting", LastContact = DateTime.Parse("2024-03-05"), OpportunityStatus = OpportunityStatus.Qualification, OpportunityPriority = OpportunityPriority.Medium },
                            new Opportunity { ID = 4, OpportunityName = "Fitness App Partnership", OpportunityAction = "Negotiate terms", POC = "Emily Davis", Account = "HealthTech", Interaction = "Conference", LastContact = DateTime.Parse("2024-04-01"), OpportunityStatus = OpportunityStatus.Negotiating, OpportunityPriority = OpportunityPriority.High },
                            new Opportunity { ID = 5, OpportunityName = "Gym Refurbishment", OpportunityAction = "Send quote", POC = "Chris Brown", Account = "Wellness Hub", Interaction = "Email", LastContact = DateTime.Parse("2024-05-20"), OpportunityStatus = OpportunityStatus.Qualification, OpportunityPriority = OpportunityPriority.Medium },
                            new Opportunity { ID = 6, OpportunityName = "Personal Training Expansion", OpportunityAction = "Confirm trainer availability", POC = "Sarah Wilson", Account = "Elite Gym", Interaction = "Phone Call", LastContact = DateTime.Parse("2024-06-12"), OpportunityStatus = OpportunityStatus.Negotiating, OpportunityPriority = OpportunityPriority.ExtremelyHigh },
                            new Opportunity { ID = 7, OpportunityName = "Equipment Maintenance Contract", OpportunityAction = "Finalize pricing", POC = "David Martinez", Account = "Pro Fitness", Interaction = "Meeting", LastContact = DateTime.Parse("2024-07-08"), OpportunityStatus = OpportunityStatus.Qualification, OpportunityPriority = OpportunityPriority.High },
                            new Opportunity { ID = 8, OpportunityName = "Yoga Studio Collaboration", OpportunityAction = "Discuss revenue share", POC = "Lisa White", Account = "Zen Studio", Interaction = "Video Call", LastContact = DateTime.Parse("2024-08-05"), OpportunityStatus = OpportunityStatus.Negotiating, OpportunityPriority = OpportunityPriority.Medium },
                            new Opportunity { ID = 9, OpportunityName = "Corporate Gym Memberships", OpportunityAction = "Send contract draft", POC = "Paul Roberts", Account = "BigCorp Ltd.", Interaction = "Email", LastContact = DateTime.Parse("2024-09-02"), OpportunityStatus = OpportunityStatus.ClosedNewMember, OpportunityPriority = OpportunityPriority.Low },
                            new Opportunity { ID = 10, OpportunityName = "Franchise Expansion", OpportunityAction = "Set up site visit", POC = "Olivia Scott", Account = "Global Gym", Interaction = "Phone Call", LastContact = DateTime.Parse("2024-10-10"), OpportunityStatus = OpportunityStatus.Qualification, OpportunityPriority = OpportunityPriority.High },
                            new Opportunity { ID = 11, OpportunityName = "Summer Bootcamp Program", OpportunityAction = "Confirm venue", POC = "James Walker", Account = "FitLife", Interaction = "Meeting", LastContact = DateTime.Parse("2024-11-15"), OpportunityStatus = OpportunityStatus.Negotiating, OpportunityPriority = OpportunityPriority.Medium },
                            new Opportunity { ID = 12, OpportunityName = "Senior Fitness Program", OpportunityAction = "Arrange trial session", POC = "Emma Hall", Account = "Golden Years", Interaction = "Phone Call", LastContact = DateTime.Parse("2024-12-01"), OpportunityStatus = OpportunityStatus.ClosedNotInterested, OpportunityPriority = OpportunityPriority.Low },
                            new Opportunity { ID = 13, OpportunityName = "Sports Nutrition Collaboration", OpportunityAction = "Discuss product bundling", POC = "Daniel Lewis", Account = "NutriMax", Interaction = "Conference", LastContact = DateTime.Parse("2025-01-18"), OpportunityStatus = OpportunityStatus.Qualification, OpportunityPriority = OpportunityPriority.High },
                            new Opportunity { ID = 14, OpportunityName = "Online Coaching Program", OpportunityAction = "Develop course content", POC = "Sophia Harris", Account = "eFit", Interaction = "Video Call", LastContact = DateTime.Parse("2025-02-05"), OpportunityStatus = OpportunityStatus.Negotiating, OpportunityPriority = OpportunityPriority.ExtremelyHigh },
                            new Opportunity { ID = 15, OpportunityName = "Gym Management Software", OpportunityAction = "Schedule software demo", POC = "Ethan Clark", Account = "TechFit", Interaction = "Email", LastContact = DateTime.Parse("2025-03-07"), OpportunityStatus = OpportunityStatus.Qualification, OpportunityPriority = OpportunityPriority.Medium },
                            new Opportunity { ID = 16, OpportunityName = "Athletic Sponsorship", OpportunityAction = "Finalize sponsorship deal", POC = "Mia Young", Account = "Pro Athletes", Interaction = "Meeting", LastContact = DateTime.Parse("2025-04-12"), OpportunityStatus = OpportunityStatus.Negotiating, OpportunityPriority = OpportunityPriority.High },
                            new Opportunity { ID = 17, OpportunityName = "Crossfit Affiliate Program", OpportunityAction = "Complete registration", POC = "Benjamin Wright", Account = "Crossfit Nation", Interaction = "Email", LastContact = DateTime.Parse("2025-05-30"), OpportunityStatus = OpportunityStatus.ClosedNewMember, OpportunityPriority = OpportunityPriority.Low },
                            new Opportunity { ID = 18, OpportunityName = "Healthy Office Initiative", OpportunityAction = "Submit proposal", POC = "Isabella King", Account = "Office Wellness", Interaction = "Phone Call", LastContact = DateTime.Parse("2025-06-10"), OpportunityStatus = OpportunityStatus.Qualification, OpportunityPriority = OpportunityPriority.High },
                            new Opportunity { ID = 19, OpportunityName = "Home Gym Equipment Sales", OpportunityAction = "Negotiate bulk pricing", POC = "Liam Allen", Account = "HomeFit", Interaction = "Video Call", LastContact = DateTime.Parse("2025-07-15"), OpportunityStatus = OpportunityStatus.Negotiating, OpportunityPriority = OpportunityPriority.Medium },
                            new Opportunity { ID = 20, OpportunityName = "University Partnership", OpportunityAction = "Develop joint fitness program", POC = "Charlotte Turner", Account = "State University", Interaction = "Meeting", LastContact = DateTime.Parse("2025-08-25"), OpportunityStatus = OpportunityStatus.Qualification, OpportunityPriority = OpportunityPriority.High }
                        );

                        // Save changes to persist the data
                        context.SaveChanges();
                    }

                    if (!context.MEvents.Any())
                    {
                        context.MEvents.AddRange(
                            new MEvent { Id = 1, EventName = "Fitness Expo", EventDescription = "Annual fitness and wellness event", EventLocation = "Convention Center", EventDate = DateTime.Parse("2024-09-15") },
                            new MEvent { Id = 2, EventName = "Yoga Retreat", EventDescription = "A weekend of yoga and mindfulness", EventLocation = "Mountain Resort", EventDate = DateTime.Parse("2024-07-10") },
                            new MEvent { Id = 3, EventName = "Health & Wellness Seminar", EventDescription = "Expert talks on nutrition and fitness", EventLocation = "Downtown Hall", EventDate = DateTime.Parse("2024-06-22") },
                            new MEvent { Id = 4, EventName = "Marathon Training Camp", EventDescription = "Coaching and preparation for marathon runners", EventLocation = "City Stadium", EventDate = DateTime.Parse("2024-05-18") },
                            new MEvent { Id = 5, EventName = "Strength Training Workshop", EventDescription = "Hands-on weightlifting techniques", EventLocation = "Fitness Gym", EventDate = DateTime.Parse("2024-08-05") }
                        );

                        context.SaveChanges();
                    }

                    if (!context.MemberEvents.Any())
                    {
                        var memberEvents = new List<MemberEvent>
                            {
                                new MemberEvent { MemberId = 1, MEventID = 1 },
                                new MemberEvent { MemberId = 2, MEventID = 2 },
                                new MemberEvent { MemberId = 2, MEventID = 1 },
                                new MemberEvent { MemberId = 3, MEventID = 3 },
                                new MemberEvent { MemberId = 4, MEventID = 4 },
                                new MemberEvent { MemberId = 5, MEventID = 5 }
                            };

                        context.MemberEvents.AddRange(memberEvents);
                        context.SaveChanges();

                    }


                    if (!context.ContactCancellations.Any())
                    {
                        context.ContactCancellations.AddRange(
                            new ContactCancellation { ID = 1, ContactID = 1, CancellationDate = DateTime.Parse("2023-05-15"), CancellationNote = "Left company", IsCancelled = true },
                            new ContactCancellation { ID = 2, ContactID = 2, CancellationDate = DateTime.Parse("2022-07-20"), CancellationNote = "Retired", IsCancelled = true },
                            new ContactCancellation { ID = 3, ContactID = 3, CancellationDate = DateTime.Parse("2024-01-10"), CancellationNote = "No longer working with us", IsCancelled = true },
                            new ContactCancellation { ID = 4, ContactID = 4, CancellationDate = DateTime.Parse("2021-11-30"), CancellationNote = "Moved to a different industry", IsCancelled = true },
                            new ContactCancellation { ID = 5, ContactID = 5, CancellationDate = DateTime.Parse("2023-09-18"), CancellationNote = "Project completed", IsCancelled = true },
                            new ContactCancellation { ID = 6, ContactID = 6, CancellationDate = DateTime.Parse("2020-12-05"), CancellationNote = "No response to emails", IsCancelled = true },
                            new ContactCancellation { ID = 7, ContactID = 7, CancellationDate = DateTime.Parse("2023-02-14"), CancellationNote = "Switched to competitor", IsCancelled = true },
                            new ContactCancellation { ID = 8, ContactID = 8, CancellationDate = DateTime.Parse("2021-06-22"), CancellationNote = "No longer interested in services", IsCancelled = true },
                            new ContactCancellation { ID = 9, ContactID = 9, CancellationDate = DateTime.Parse("2022-09-07"), CancellationNote = "Business shutdown", IsCancelled = true },
                            new ContactCancellation { ID = 10, ContactID = 10, CancellationDate = DateTime.Parse("2024-02-01"), CancellationNote = "Contract expired", IsCancelled = true },
                            new ContactCancellation { ID = 11, ContactID = 11, CancellationDate = DateTime.Parse("2023-03-12"), CancellationNote = "Company acquisition", IsCancelled = true },
                            new ContactCancellation { ID = 12, ContactID = 12, CancellationDate = DateTime.Parse("2022-05-30"), CancellationNote = "No longer in the role", IsCancelled = true },
                            new ContactCancellation { ID = 13, ContactID = 13, CancellationDate = DateTime.Parse("2020-09-10"), CancellationNote = "Repeated complaints", IsCancelled = true },
                            new ContactCancellation { ID = 14, ContactID = 14, CancellationDate = DateTime.Parse("2023-07-25"), CancellationNote = "Moved to a competitor", IsCancelled = true }
                            );
                        context.SaveChanges();
                    }

                    if (!context.AnnualActions.Any())
                    {
                        context.AnnualActions.AddRange(
                            new AnnualAction { ID = 1, Name = "Budget Planning", Note = "Finalize annual budget", Date = DateTime.Parse("2025-01-10"), Asignee = "John Doe", AnnualStatus = AnnualStatus.ToDo },
                            new AnnualAction { ID = 2, Name = "Team Meeting", Note = "Kickoff meeting for new year", Date = DateTime.Parse("2025-01-15"), Asignee = "Jane Smith", AnnualStatus = AnnualStatus.InProgress },
                            new AnnualAction { ID = 3, Name = "Client Outreach", Note = "Send holiday greetings", Date = DateTime.Parse("2024-12-20"), Asignee = "Michael Johnson", AnnualStatus = AnnualStatus.Done },
                            new AnnualAction { ID = 4, Name = "System Maintenance", Note = "Upgrade CRM software", Date = DateTime.Parse("2025-02-01"), Asignee = "Emily Davis", AnnualStatus = AnnualStatus.Cancelled },
                            new AnnualAction { ID = 5, Name = "Annual Report", Note = "Prepare financial report", Date = DateTime.Parse("2025-03-05"), Asignee = "David Martinez", AnnualStatus = AnnualStatus.ToDo },
                            new AnnualAction { ID = 6, Name = "Employee Training", Note = "Conduct compliance training", Date = DateTime.Parse("2025-04-10"), Asignee = "Sarah Wilson", AnnualStatus = AnnualStatus.InProgress },
                            new AnnualAction { ID = 7, Name = "Product Launch", Note = "Launch new software version", Date = DateTime.Parse("2025-05-15"), Asignee = "Robert Brown", AnnualStatus = AnnualStatus.ToDo },
                            new AnnualAction { ID = 8, Name = "Security Audit", Note = "Perform yearly security review", Date = DateTime.Parse("2025-06-20"), Asignee = "Jessica Lee", AnnualStatus = AnnualStatus.Cancelled },
                            new AnnualAction { ID = 9, Name = "Client Feedback", Note = "Gather customer reviews", Date = DateTime.Parse("2025-07-10"), Asignee = "Daniel White", AnnualStatus = AnnualStatus.Done },
                            new AnnualAction { ID = 10, Name = "Marketing Strategy", Note = "Plan Q3 campaigns", Date = DateTime.Parse("2025-08-05"), Asignee = "Olivia Harris", AnnualStatus = AnnualStatus.InProgress },
                            new AnnualAction { ID = 11, Name = "IT Infrastructure", Note = "Upgrade cloud servers", Date = DateTime.Parse("2025-09-12"), Asignee = "Liam Scott", AnnualStatus = AnnualStatus.ToDo },
                            new AnnualAction { ID = 12, Name = "Year-End Review", Note = "Assess team performance", Date = DateTime.Parse("2025-10-25"), Asignee = "Sophia Nelson", AnnualStatus = AnnualStatus.Cancelled },
                            new AnnualAction { ID = 13, Name = "Partnership Meeting", Note = "Discuss collaboration", Date = DateTime.Parse("2025-11-07"), Asignee = "Benjamin Carter", AnnualStatus = AnnualStatus.Done },
                            new AnnualAction { ID = 14, Name = "Recruitment Drive", Note = "Hire new developers", Date = DateTime.Parse("2025-12-15"), Asignee = "Charlotte Adams", AnnualStatus = AnnualStatus.InProgress },
                            new AnnualAction { ID = 15, Name = "Office Renovation", Note = "Upgrade workspace", Date = DateTime.Parse("2026-01-10"), Asignee = "Ethan Walker", AnnualStatus = AnnualStatus.Cancelled },
                            new AnnualAction { ID = 16, Name = "Customer Support Training", Note = "Improve service quality", Date = DateTime.Parse("2026-02-20"), Asignee = "Mia Rodriguez", AnnualStatus = AnnualStatus.ToDo },
                            new AnnualAction { ID = 17, Name = "Industry Conference", Note = "Attend and network", Date = DateTime.Parse("2026-03-05"), Asignee = "Noah Stewart", AnnualStatus = AnnualStatus.Done },
                            new AnnualAction { ID = 18, Name = "Legal Compliance", Note = "Update contracts", Date = DateTime.Parse("2026-04-15"), Asignee = "Ava Perez", AnnualStatus = AnnualStatus.ToDo },
                            new AnnualAction { ID = 19, Name = "Server Migration", Note = "Move to new data center", Date = DateTime.Parse("2026-05-10"), Asignee = "James Mitchell", AnnualStatus = AnnualStatus.InProgress },
                            new AnnualAction { ID = 20, Name = "Policy Update", Note = "Revise HR policies", Date = DateTime.Parse("2026-06-18"), Asignee = "Isabella Turner", AnnualStatus = AnnualStatus.Done },
                            new AnnualAction { ID = 21, Name = "Product Testing", Note = "QA for new release", Date = DateTime.Parse("2026-07-22"), Asignee = "Mason Hill", AnnualStatus = AnnualStatus.Cancelled },
                            new AnnualAction { ID = 22, Name = "Social Media Campaign", Note = "Boost brand awareness", Date = DateTime.Parse("2026-08-30"), Asignee = "Harper Green", AnnualStatus = AnnualStatus.InProgress },
                            new AnnualAction { ID = 23, Name = "Inventory Check", Note = "Verify stock levels", Date = DateTime.Parse("2026-09-14"), Asignee = "Alexander Young", AnnualStatus = AnnualStatus.Done },
                            new AnnualAction { ID = 24, Name = "Client Contracts", Note = "Renew key contracts", Date = DateTime.Parse("2026-10-10"), Asignee = "Ella Baker", AnnualStatus = AnnualStatus.ToDo },
                            new AnnualAction { ID = 25, Name = "Sustainability Report", Note = "Prepare environmental report", Date = DateTime.Parse("2026-11-05"), Asignee = "Lucas Evans", AnnualStatus = AnnualStatus.Done }
                        );

                        context.SaveChanges();
                    }

                    if (!context.Strategies.Any())
                    {
                        context.Strategies.AddRange(
                            new Strategy { ID = 1, StrategyName = "Market Expansion", StrategyAssignee = "John Doe", StrategyNote = "Expand into new regions", CreatedDate = DateTime.Parse("2025-01-10"), StrategyTerm = StrategyTerm.LongTerm, StrategyStatus = StrategyStatus.ToDo },
                            new Strategy { ID = 2, StrategyName = "Cost Reduction", StrategyAssignee = "Jane Smith", StrategyNote = "Optimize supply chain", CreatedDate = DateTime.Parse("2025-02-15"), StrategyTerm = StrategyTerm.MediumTerm, StrategyStatus = StrategyStatus.InProgress },
                            new Strategy { ID = 3, StrategyName = "Product Innovation", StrategyAssignee = "Michael Johnson", StrategyNote = "Develop new features", CreatedDate = DateTime.Parse("2025-03-20"), StrategyTerm = StrategyTerm.ShortTerm, StrategyStatus = StrategyStatus.Done },
                            new Strategy { ID = 4, StrategyName = "Employee Wellness", StrategyAssignee = "Emily Davis", StrategyNote = "Enhance benefits package", CreatedDate = DateTime.Parse("2025-04-01"), StrategyTerm = StrategyTerm.LongTerm, StrategyStatus = StrategyStatus.Cancelled },
                            new Strategy { ID = 5, StrategyName = "Sustainability Initiative", StrategyAssignee = "David Martinez", StrategyNote = "Reduce carbon footprint", CreatedDate = DateTime.Parse("2025-05-05"), StrategyTerm = StrategyTerm.Seasonal, StrategyStatus = StrategyStatus.ToDo },
                            new Strategy { ID = 6, StrategyName = "Cybersecurity Upgrade", StrategyAssignee = "Sarah Wilson", StrategyNote = "Implement MFA & encryption", CreatedDate = DateTime.Parse("2025-06-10"), StrategyTerm = StrategyTerm.LongTerm, StrategyStatus = StrategyStatus.InProgress },
                            new Strategy { ID = 7, StrategyName = "Customer Retention", StrategyAssignee = "Robert Brown", StrategyNote = "Loyalty program launch", CreatedDate = DateTime.Parse("2025-07-15"), StrategyTerm = StrategyTerm.MediumTerm, StrategyStatus = StrategyStatus.ToDo },
                            new Strategy { ID = 8, StrategyName = "Digital Marketing Expansion", StrategyAssignee = "Jessica Lee", StrategyNote = "Increase social media ads", CreatedDate = DateTime.Parse("2025-08-20"), StrategyTerm = StrategyTerm.ShortTerm, StrategyStatus = StrategyStatus.Cancelled },
                            new Strategy { ID = 9, StrategyName = "Workforce Upskilling", StrategyAssignee = "Daniel White", StrategyNote = "Train employees on AI tools", CreatedDate = DateTime.Parse("2025-09-10"), StrategyTerm = StrategyTerm.Cyclical, StrategyStatus = StrategyStatus.Done },
                            new Strategy { ID = 10, StrategyName = "AI Integration", StrategyAssignee = "Olivia Harris", StrategyNote = "Enhance automation", CreatedDate = DateTime.Parse("2025-10-05"), StrategyTerm = StrategyTerm.LongTerm, StrategyStatus = StrategyStatus.InProgress },
                            new Strategy { ID = 11, StrategyName = "Brand Repositioning", StrategyAssignee = "Liam Scott", StrategyNote = "Redefine brand image", CreatedDate = DateTime.Parse("2025-11-12"), StrategyTerm = StrategyTerm.NotStated, StrategyStatus = StrategyStatus.ToDo },
                            new Strategy { ID = 12, StrategyName = "Community Engagement", StrategyAssignee = "Sophia Nelson", StrategyNote = "Increase local partnerships", CreatedDate = DateTime.Parse("2025-12-25"), StrategyTerm = StrategyTerm.Seasonal, StrategyStatus = StrategyStatus.Cancelled },
                            new Strategy { ID = 13, StrategyName = "Research & Development", StrategyAssignee = "Benjamin Carter", StrategyNote = "Explore emerging technologies", CreatedDate = DateTime.Parse("2026-01-07"), StrategyTerm = StrategyTerm.LongTerm, StrategyStatus = StrategyStatus.Done },
                            new Strategy { ID = 14, StrategyName = "Subscription Model", StrategyAssignee = "Charlotte Adams", StrategyNote = "Introduce tiered pricing", CreatedDate = DateTime.Parse("2026-02-15"), StrategyTerm = StrategyTerm.MediumTerm, StrategyStatus = StrategyStatus.InProgress },
                            new Strategy { ID = 15, StrategyName = "Remote Work Policy", StrategyAssignee = "Ethan Walker", StrategyNote = "Hybrid work approach", CreatedDate = DateTime.Parse("2026-03-10"), StrategyTerm = StrategyTerm.ShortTerm, StrategyStatus = StrategyStatus.Cancelled },
                            new Strategy { ID = 16, StrategyName = "E-commerce Expansion", StrategyAssignee = "Mia Rodriguez", StrategyNote = "Launch direct-to-consumer site", CreatedDate = DateTime.Parse("2026-04-20"), StrategyTerm = StrategyTerm.Cyclical, StrategyStatus = StrategyStatus.ToDo },
                            new Strategy { ID = 17, StrategyName = "Supplier Diversification", StrategyAssignee = "Noah Stewart", StrategyNote = "Reduce dependency on single supplier", CreatedDate = DateTime.Parse("2026-05-05"), StrategyTerm = StrategyTerm.MediumTerm, StrategyStatus = StrategyStatus.Done },
                            new Strategy { ID = 18, StrategyName = "Partnership Development", StrategyAssignee = "Ava Perez", StrategyNote = "Form strategic alliances", CreatedDate = DateTime.Parse("2026-06-15"), StrategyTerm = StrategyTerm.LongTerm, StrategyStatus = StrategyStatus.ToDo },
                            new Strategy { ID = 19, StrategyName = "Retail Store Optimization", StrategyAssignee = "James Mitchell", StrategyNote = "Increase in-store sales", CreatedDate = DateTime.Parse("2026-07-10"), StrategyTerm = StrategyTerm.ShortTerm, StrategyStatus = StrategyStatus.InProgress },
                            new Strategy { ID = 20, StrategyName = "Omnichannel Strategy", StrategyAssignee = "Isabella Turner", StrategyNote = "Integrate offline & online sales", CreatedDate = DateTime.Parse("2026-08-18"), StrategyTerm = StrategyTerm.Seasonal, StrategyStatus = StrategyStatus.Done },
                            new Strategy { ID = 21, StrategyName = "Warehouse Automation", StrategyAssignee = "Mason Hill", StrategyNote = "Use AI-driven logistics", CreatedDate = DateTime.Parse("2026-09-22"), StrategyTerm = StrategyTerm.Cyclical, StrategyStatus = StrategyStatus.Cancelled },
                            new Strategy { ID = 22, StrategyName = "Expansion to New Markets", StrategyAssignee = "Harper Green", StrategyNote = "Target Asia-Pacific region", CreatedDate = DateTime.Parse("2026-10-30"), StrategyTerm = StrategyTerm.LongTerm, StrategyStatus = StrategyStatus.InProgress },
                            new Strategy { ID = 23, StrategyName = "Influencer Marketing", StrategyAssignee = "Alexander Young", StrategyNote = "Partner with key influencers", CreatedDate = DateTime.Parse("2026-11-14"), StrategyTerm = StrategyTerm.ShortTerm, StrategyStatus = StrategyStatus.Done },
                            new Strategy { ID = 24, StrategyName = "Customer Data Analytics", StrategyAssignee = "Ella Baker", StrategyNote = "Leverage insights for marketing", CreatedDate = DateTime.Parse("2026-12-10"), StrategyTerm = StrategyTerm.NotStated, StrategyStatus = StrategyStatus.ToDo },
                            new Strategy { ID = 25, StrategyName = "Financial Restructuring", StrategyAssignee = "Lucas Evans", StrategyNote = "Optimize revenue streams", CreatedDate = DateTime.Parse("2027-01-05"), StrategyTerm = StrategyTerm.MediumTerm, StrategyStatus = StrategyStatus.Done }
                        );

                        context.SaveChanges();
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);

                }


                #endregion
            }

        }


    }
}
