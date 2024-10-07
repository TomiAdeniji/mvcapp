using CleanBooksData;
using Microsoft.AspNet.Identity.CoreCompat;
using Qbicles.Models;
using Qbicles.Models.Bookkeeping;
using Qbicles.Models.Community;
using Qbicles.Models.FileStorage;
using Qbicles.Models.Form;
using Qbicles.Models.Invitation;
using Qbicles.Models.Manufacturing;
using Qbicles.Models.SalesMkt;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Inventory;
using Qbicles.Models.Trader.Movement;
using Qbicles.Models.Trader.PoS;
using Qbicles.Models.Trader.Pricing;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using Qbicles.Models.Trader.Payments;
using Qbicles.Models.Trader.TraderWorkgroup;
using Qbicles.Models.Trader.Budgets;
using Qbicles.Models.Trader.DDS;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Trader.Resources;
using Qbicles.Models.Trader.Returns;
using Qbicles.Models.Spannered;
using Qbicles.Models.Attributes;
using Qbicles.Models.Operator;
using Qbicles.Models.Operator.Compliance;
using Qbicles.Models.Operator.Goals;
using Qbicles.Models.Operator.Team;
using Qbicles.Models.Operator.TimeAttendance;
using Qbicles.Models.Broadcast;
using Qbicles.Models.Trader.CashMgt;
using Qbicles.Models.Trader.Reorder;
using Qbicles.Models.Trader.Settings;
using Qbicles.Models.B2B;
using Qbicles.Models.SystemDomain;
using Qbicles.Models.MyBankMate;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Highlight;
using Qbicles.Models.Trader.SalesChannel;
using Qbicles.Models.Catalogs;
using Qbicles.Models.UserInformation;
using Qbicles.Models.Network;
using Qbicles.Models.ProfilePage;
using Qbicles.Models.Loyalty;
using Qbicles.Models.Qbicles;
using Qbicles.Models.OrderLogging;
using Qbicles.Models.Paystack;
using Qbicles.Models.ProductSearch;
using Qbicles.Models.Trader.Product;
using Qbicles.Models.WaitList;
using Qbicles.Models.Firebase;
using System;
using Attribute = Qbicles.Models.SalesMkt.Attribute;
using System.Threading.Tasks;
using System.Threading;

namespace Qbicles.BusinessRules.Model
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
            Database.Log = Console.WriteLine;
            //((IObjectContextAdapter)this).ObjectContext.ObjectMaterialized += ObjectMaterialized;
            Configuration.LazyLoadingEnabled = true;
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        private static bool IsReferenceAndNotLoaded(DbEntityEntry entry, string memberName)
        {
            var reference = entry.Member(memberName) as DbReferenceEntry;
            return reference != null && !reference.IsLoaded;
        }

        protected override DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry,
            IDictionary<object, object> items)
        {
            var result = base.ValidateEntity(entityEntry, items);
            if (result.IsValid || entityEntry.State != EntityState.Modified) return result;
            return new DbEntityValidationResult(entityEntry,
                result.ValidationErrors
                    .Where(e => !IsReferenceAndNotLoaded(entityEntry, e.PropertyName)));
        }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
        }

        //public override int SaveChanges()

        //{
        //    var contextAdapter = (IObjectContextAdapter)this;

        //    contextAdapter.ObjectContext.DetectChanges(); //force this. Sometimes entity state needs a handle jiggle

        //    var pendingEntities = contextAdapter.ObjectContext.ObjectStateManager
        //        .GetObjectStateEntries(EntityState.Added | EntityState.Modified)
        //        .Where(en => !en.IsRelationship).ToList();

        //    foreach (var entry in pendingEntities) //Encrypt all pending changes

        //        EncryptEntity(entry.Entity);

        //    var result = base.SaveChanges();

        //    foreach (var entry in pendingEntities) //Decrypt updated entities for continued use

        //        DecryptEntity(entry.Entity);

        //    return result;
        //}

        //public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)

        //{
        //    var contextAdapter = (IObjectContextAdapter)this;

        //    contextAdapter.ObjectContext.DetectChanges(); //force this. Sometimes entity state needs a handle jiggle

        //    var pendingEntities = contextAdapter.ObjectContext.ObjectStateManager
        //        .GetObjectStateEntries(EntityState.Added | EntityState.Modified)
        //        .Where(en => !en.IsRelationship).ToList();

        //    foreach (var entry in pendingEntities) //Encrypt all pending changes

        //        EncryptEntity(entry.Entity);

        //    var result = await base.SaveChangesAsync(cancellationToken);

        //    foreach (var entry in pendingEntities) //Decrypt updated entities for continued use

        //        DecryptEntity(entry.Entity);

        //    return result;
        //}

        //private void ObjectMaterialized(object sender, ObjectMaterializedEventArgs e)

        //{
        //    DecryptEntity(e.Entity);
        //}

        //private void EncryptEntity(object entity)

        //{
        //    //Get all the properties that are encryptable and encrypt them

        //    var encryptedProperties = entity.GetType().GetProperties()
        //        .Where(p => p.GetCustomAttributes(typeof(EncryptedAttribute), true)
        //            .Any(a => p.PropertyType == typeof(string)));

        //    foreach (var property in encryptedProperties)

        //    {
        //        var value = property.GetValue(entity) as string;

        //        if (!string.IsNullOrEmpty(value))

        //        {
        //            var encryptedValue = value.Encrypt(ENCRYPTION_KEY);

        //            property.SetValue(entity, encryptedValue);
        //        }
        //    }
        //}

        //private void DecryptEntity(object entity)

        //{
        //    //Get all the properties that are encryptable and decyrpt them

        //    var encryptedProperties = entity.GetType().GetProperties()
        //        .Where(p => p.GetCustomAttributes(typeof(EncryptedAttribute), true)
        //            .Any(a => p.PropertyType == typeof(string)));

        //    foreach (var property in encryptedProperties)

        //    {
        //        var encryptedValue = property.GetValue(entity) as string;

        //        if (!string.IsNullOrEmpty(encryptedValue))

        //        {
        //            var value = encryptedValue.Decrypt(ENCRYPTION_KEY);

        //            Entry(entity).Property(property.Name).OriginalValue = value;

        //            Entry(entity).Property(property.Name).IsModified = false;
        //        }
        //    }
        //}

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configurations.Add(new ViewDatesQbicleStreamConfiguration());
            modelBuilder.Configurations.Add(new ViewQbicleActivitiesStreamConfiguration());
            modelBuilder.Configurations.Add(new UnusedInventoryViewConfiguration());
            modelBuilder.Configurations.Add(new UnusedBatchesViewConfiguration());
            modelBuilder.Entity<IdentityUser>().ToTable("Users");
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");

            modelBuilder.Conventions.Add(new DecimalPrecisionAttributeConvention());

            //modelBuilder.Entity<Microsoft.AspNet.Identity.CoreCompat.>().ToTable("UserRoles");
            //modelBuilder.Entity<Microsoft.AspNet.Identity.CoreCompat.IdentityRoleClaim>().ToTable("UserClaims");
            //modelBuilder.Entity<Microsoft.AspNet.Identity.CoreCompat.IdentityUserLogin>().ToTable("UserLogins");

            // Setup the many to many relationships explicitly.
            // It appears that the Many To Many Relations are not being setup for MySql through the classes add-migration

            #region Cleanbooks

            modelBuilder.Entity<Account>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<Account>()
                .Property(e => e.Number)
                .IsUnicode(false);

            modelBuilder.Entity<Account>()
                .Property(e => e.CreatedById)
                .IsUnicode(false);

            modelBuilder.Entity<Account>()
                .Property(e => e.DataManagerId)
                .IsUnicode(false);

            modelBuilder.Entity<Account>()
                .HasMany(e => e.deleteduploads)
                .WithRequired(e => e.account)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Account>()
                .HasMany(e => e.taskaccounts)
                .WithRequired(e => e.account)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Account>()
                .HasMany(e => e.uploads)
                .WithRequired(e => e.account)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Account>()
                .HasMany(e => e.uploadfields)
                .WithRequired(e => e.account)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<accountgroup>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<accountgroup>()
                .Property(e => e.CreatedById)
                .IsUnicode(false);

            modelBuilder.Entity<accountgroup>()
                .Property(e => e.LogoPath)
                .IsUnicode(false);

            modelBuilder.Entity<accountgroup>()
                .HasMany(e => e.projects)
                .WithOptional(e => e.accountgroup)
                .HasForeignKey(e => e.AssignedAccountGroupId);

            modelBuilder.Entity<accountgroup>()
                .HasMany(e => e.Accounts)
                .WithRequired(e => e.accountgroup)
                .HasForeignKey(e => e.GroupId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<QbicleDomain>()
                .HasMany(e => e.accountgroups)
                .WithRequired(e => e.qbicledomain)
                .HasForeignKey(e => e.DomainId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<accountupdatefrequency>()
                .Property(e => e.frequency)
                .IsUnicode(false);

            modelBuilder.Entity<accountupdatefrequency>()
                .HasMany(e => e.Accounts)
                .WithOptional(e => e.accountupdatefrequency)
                .HasForeignKey(e => e.UpdateFrequencyId);

            modelBuilder.Entity<dateformat>()
                .Property(e => e.Format)
                .IsUnicode(false);

            modelBuilder.Entity<dateformat>()
                .Property(e => e.DateFormatExpression)
                .IsUnicode(false);

            modelBuilder.Entity<dateformat>()
                .Property(e => e.CreatedById)
                .IsUnicode(false);

            modelBuilder.Entity<deletedaccount>()
                .Property(e => e.AccountName)
                .IsUnicode(false);

            modelBuilder.Entity<deletedaccount>()
                .Property(e => e.AccountNumber)
                .IsUnicode(false);

            modelBuilder.Entity<deletedaccount>()
                .Property(e => e.CreatedById)
                .IsUnicode(false);

            modelBuilder.Entity<deletedaccount>()
                .Property(e => e.DeleteById)
                .IsUnicode(false);

            modelBuilder.Entity<deletedtask>()
                .Property(e => e.TaskName)
                .IsUnicode(false);

            modelBuilder.Entity<deletedtask>()
                .Property(e => e.TaskDescription)
                .IsUnicode(false);

            modelBuilder.Entity<deletedtask>()
                .Property(e => e.DeletedById)
                .IsUnicode(false);

            modelBuilder.Entity<deletedupload>()
                .Property(e => e.UploadName)
                .IsUnicode(false);

            modelBuilder.Entity<deletedupload>()
                .Property(e => e.DeletedById)
                .IsUnicode(false);

            modelBuilder.Entity<deletedupload>()
                .Property(e => e.NumberOfTransactions)
                .IsUnicode(false);

            modelBuilder.Entity<filetype>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<profile>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<profile>()
                .Property(e => e.Flag)
                .IsUnicode(false);

            modelBuilder.Entity<profile>()
                .Property(e => e.Expression)
                .IsUnicode(false);

            modelBuilder.Entity<profile>()
                .Property(e => e.CreatedById)
                .IsUnicode(false);

            modelBuilder.Entity<profile>()
                .Property(e => e.ModifiedById)
                .IsUnicode(false);

            modelBuilder.Entity<profile>()
                .HasMany(e => e.transactionanalysistaskprofilexrefs)
                .WithRequired(e => e.profile)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<project>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<project>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<project>()
                .Property(e => e.CreatedById)
                .IsUnicode(false);

            modelBuilder.Entity<project>()
                .HasMany(e => e.projecttaskxrefs)
                .WithRequired(e => e.project)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<projectgroup>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<projectgroup>()
                .Property(e => e.CreatedById)
                .IsUnicode(false);

            modelBuilder.Entity<projectgroup>()
                .HasMany(e => e.projects)
                .WithRequired(e => e.projectgroup)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<projectnotificationinterval>()
                .Property(e => e.Interval)
                .IsUnicode(false);

            modelBuilder.Entity<projectnotificationinterval>()
                .HasMany(e => e.projects)
                .WithRequired(e => e.projectnotificationinterval)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<reasonsentemail>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<scheduledemail>()
                .Property(e => e.Body)
                .IsUnicode(false);
            modelBuilder.Entity<scheduledaccountemail>()
                .Property(e => e.Body)
                .IsUnicode(false);
            modelBuilder.Entity<task>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<task>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<task>()
                .Property(e => e.CreatedById)
                .IsUnicode(false);

            modelBuilder.Entity<task>()
                .Property(e => e.AssignedUserId)
                .IsUnicode(false);

            modelBuilder.Entity<task>()
                .HasMany(e => e.taskaccounts)
                .WithRequired(e => e.task)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<task>()
                .HasMany(e => e.taskinstances)
                .WithRequired(e => e.task)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<taskexecutioninterval>()
                .Property(e => e.Interval)
                .IsUnicode(false);

            modelBuilder.Entity<taskexecutioninterval>()
                .HasMany(e => e.tasks)
                .WithRequired(e => e.taskexecutioninterval)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<taskgroup>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<taskgroup>()
                .Property(e => e.CreatedById)
                .IsUnicode(false);

            modelBuilder.Entity<taskgroup>()
                .HasMany(e => e.tasks)
                .WithRequired(e => e.taskgroup)
                .HasForeignKey(e => e.ReconciliationTaskGroupId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<taskinstance>()
                .Property(e => e.ExecutedById)
                .IsUnicode(false);

            modelBuilder.Entity<taskinstance>()
                .HasMany(e => e.transactionanalysistasks)
                .WithRequired(e => e.taskinstance)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<taskinstance>()
                .HasMany(e => e.transactionmatchingtasks)
                .WithRequired(e => e.taskinstance)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<taskinstancedaterange>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<tasktype>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<tasktype>()
                .HasMany(e => e.tasks)
                .WithRequired(e => e.tasktype)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<transaction>()
                .Property(e => e.Reference)
                .IsUnicode(false);

            modelBuilder.Entity<transaction>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<transaction>()
                .Property(e => e.Reference1)
                .IsUnicode(false);

            modelBuilder.Entity<transaction>()
                .Property(e => e.DescCol1)
                .IsUnicode(false);

            modelBuilder.Entity<transaction>()
                .Property(e => e.DescCol2)
                .IsUnicode(false);

            modelBuilder.Entity<transaction>()
                .Property(e => e.DescCol3)
                .IsUnicode(false);

            modelBuilder.Entity<transaction>()
                .HasMany(e => e.transactionanalysisrecords)
                .WithRequired(e => e.transaction)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<transaction>()
                .HasMany(e => e.transactionmatchingrecords)
                .WithRequired(e => e.transaction)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<transactionanalysisclassificationbyrange>()
                .Property(e => e.ProfileType)
                .IsUnicode(false);

            modelBuilder.Entity<transactionanalysisclassificationbytype>()
                .Property(e => e.ProfileType)
                .IsUnicode(false);

            modelBuilder.Entity<transactionanalysiscomment>()
                .Property(e => e.Comment)
                .IsUnicode(false);

            modelBuilder.Entity<transactionanalysiscomment>()
                .Property(e => e.CreatedBy)
                .IsUnicode(false);

            modelBuilder.Entity<transactionanalysisrecord>()
                .Property(e => e.ProfileValue)
                .IsUnicode(false);

            modelBuilder.Entity<transactionanalysisreportgroup>()
                .Property(e => e.PanelTitle)
                .IsUnicode(false);

            modelBuilder.Entity<transactionanalysisreportgroup>()
                .HasMany(e => e.transactionanalysisclassificationbydates)
                .WithRequired(e => e.transactionanalysisreportgroup)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<transactionanalysisreportgroup>()
                .HasMany(e => e.transactionanalysisclassificationbyranges)
                .WithRequired(e => e.transactionanalysisreportgroup)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<transactionanalysisreportgroup>()
                .HasMany(e => e.transactionanalysisclassificationbytypes)
                .WithRequired(e => e.transactionanalysisreportgroup)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<transactionanalysisreportgroup>()
                .HasMany(e => e.transactionanalysiscomments)
                .WithRequired(e => e.transactionanalysisreportgroup)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<transactionanalysisreportgroup>()
                .HasMany(e => e.transactionanalysisreportstatistics)
                .WithRequired(e => e.transactionanalysisreportgroup)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<transactionanalysistask>()
                .HasMany(e => e.transactionanalysisrecords)
                .WithRequired(e => e.transactionanalysistask)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<transactionanalysistask>()
                .HasMany(e => e.transactionanalysisreportgroups)
                .WithRequired(e => e.transactionanalysistask)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<transactionanalysistask>()
                .HasMany(e => e.transactionanalysistaskprofilexrefs)
                .WithRequired(e => e.transactionanalysistask)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<transactionmatchinggroup>()
                .HasMany(e => e.transactionmatchingmatcheds)
                .WithRequired(e => e.transactionmatchinggroup)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<transactionmatchingmethod>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<transactionmatchingmethod>()
                .HasMany(e => e.transactionmatchinggroups)
                .WithRequired(e => e.transactionmatchingmethod)
                .HasForeignKey(e => e.TransactionMatchMethodID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<transactionmatchingrecord>()
                .HasMany(e => e.transactionmatchingcopiedrecords)
                .WithRequired(e => e.transactionmatchingrecord)
                .HasForeignKey(e => e.TransactionMatchingRecordId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<transactionmatchingrecord>()
                .HasMany(e => e.transactionmatchingcopiedrecords1)
                .WithRequired(e => e.transactionmatchingrecord1)
                .HasForeignKey(e => e.CopiedFromId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<transactionmatchingrecord>()
                .HasMany(e => e.transactionmatchingmatcheds)
                .WithRequired(e => e.transactionmatchingrecord)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<transaction>()
                .HasMany(e => e.transactionmatchingunmatcheds)
                .WithRequired(e => e.transactions)
                .HasForeignKey(e => e.TransactionId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<task>()
                .HasMany(e => e.transactionmatchingunmatcheds)
                .WithRequired(e => e.tasks)
                .HasForeignKey(e => e.TaskId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<transactionmatchingunmatchgroup>()
                .HasMany(e => e.transactionmatchingunmatcheds)
                .WithRequired(e => e.transactionmatchingunmatchgroups)
                .HasForeignKey(e => e.TransactionMatchingUnMatchGroupId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<transactionmatchingrelationship>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<transactionmatchingrelationship>()
                .HasMany(e => e.transactionmatchinggroups)
                .WithRequired(e => e.transactionmatchingrelationship)
                .HasForeignKey(e => e.TransactionMatchRelationshipId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<transactionmatchingtask>()
                .HasMany(e => e.transactionmatchingrecords)
                .WithRequired(e => e.transactionmatchingtask)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<transactionmatchingtype>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<transactionmatchingtype>()
                .HasMany(e => e.tasks)
                .WithRequired(e => e.transactionmatchingtype)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<upload>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<upload>()
                .Property(e => e.FileName)
                .IsUnicode(false);

            modelBuilder.Entity<upload>()
                .Property(e => e.FilePath)
                .IsUnicode(false);

            modelBuilder.Entity<upload>()
                .Property(e => e.CreatedById)
                .IsUnicode(false);

            modelBuilder.Entity<upload>()
                .HasMany(e => e.transactions)
                .WithRequired(e => e.upload)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<uploadfield>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<UploadFormat>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<UploadFormat>()
                .Property(e => e.CSVDelimiterValue)
                .IsUnicode(false);

            modelBuilder.Entity<UploadFormat>()
                .Property(e => e.CreatedById)
                .IsUnicode(false);

            modelBuilder.Entity<UploadFormat>()
                .HasMany(e => e.uploads)
                .WithRequired(e => e.UploadFormat)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .Property(e => e.Id)
                .IsUnicode(false);

            modelBuilder.Entity<ApplicationUser>()
                .Property(e => e.Email)
                .IsUnicode(false);

            modelBuilder.Entity<ApplicationUser>()
                .Property(e => e.PasswordHash)
                .IsUnicode(false);

            modelBuilder.Entity<ApplicationUser>()
                .Property(e => e.SecurityStamp)
                .IsUnicode(false);

            modelBuilder.Entity<ApplicationUser>()
                .Property(e => e.PhoneNumber)
                .IsUnicode(false);

            modelBuilder.Entity<ApplicationUser>()
                .Property(e => e.UserName)
                .IsUnicode(false);

            modelBuilder.Entity<ApplicationUser>()
                .Property(e => e.Profile)
                .IsUnicode(false);

            modelBuilder.Entity<ApplicationUser>()
                .Property(e => e.ProfilePic)
                .IsUnicode(false);
            modelBuilder.Entity<audit_user>()
                .Property(e => e.User1_id)
                .IsUnicode(false);
            modelBuilder.Entity<audit_user>()
                .Property(e => e.User2_id)
                .IsUnicode(false);
            modelBuilder.Entity<audit_account>()
                .Property(e => e.Name)
                .IsUnicode(false);
            modelBuilder.Entity<audit_account>()
                .Property(e => e.Name_New)
                .IsUnicode(false);
            modelBuilder.Entity<audit_account>()
                .Property(e => e.Number)
                .IsUnicode(false);
            modelBuilder.Entity<audit_account>()
                .Property(e => e.Number_New)
                .IsUnicode(false);
            modelBuilder.Entity<audit_account>()
                .Property(e => e.Number)
                .IsUnicode(false);
            modelBuilder.Entity<audit_task>()
                .Property(e => e.Name)
                .IsUnicode(false);
            modelBuilder.Entity<audit_task>()
                .Property(e => e.Name_New)
                .IsUnicode(false);
            modelBuilder.Entity<audit_transaction_analysis>()
                .Property(e => e.User_id)
                .IsUnicode(false);
            modelBuilder.Entity<audit_transaction_matching>()
                .Property(e => e.User_id)
                .IsUnicode(false);
            modelBuilder.Entity<transactionmatchingtaskrulesacces>()
                .Property(e => e.Id);
            modelBuilder.Entity<transactionmatchingrule>()
                .Property(e => e.VarianceName).IsUnicode(false);
            modelBuilder.Entity<transactionmatchingamountvariancevalue>()
                .Property(e => e.Id);
            modelBuilder.Entity<transactionmatchingdatevariancevalue>()
                .Property(e => e.Id);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.Accounts)
                .WithOptional(e => e.user)
                .HasForeignKey(e => e.DataManagerId);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.accounts1)
                .WithRequired(e => e.user1)
                .HasForeignKey(e => e.CreatedById)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.accountgroups)
                .WithRequired(e => e.user)
                .HasForeignKey(e => e.CreatedById)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.dateformats)
                .WithRequired(e => e.user)
                .HasForeignKey(e => e.CreatedById)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.deletedaccounts)
                .WithRequired(e => e.user)
                .HasForeignKey(e => e.CreatedById)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.deletedaccounts1)
                .WithRequired(e => e.user1)
                .HasForeignKey(e => e.DeleteById)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.deletedtasks)
                .WithRequired(e => e.user)
                .HasForeignKey(e => e.DeletedById)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.deleteduploads)
                .WithRequired(e => e.user)
                .HasForeignKey(e => e.DeletedById)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.profiles)
                .WithOptional(e => e.user)
                .HasForeignKey(e => e.CreatedById);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.profiles1)
                .WithOptional(e => e.user1)
                .HasForeignKey(e => e.ModifiedById);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.projects)
                .WithRequired(e => e.user)
                .HasForeignKey(e => e.CreatedById)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.projectgroups)
                .WithRequired(e => e.user)
                .HasForeignKey(e => e.CreatedById)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.tasks)
                .WithRequired(e => e.user)
                .HasForeignKey(e => e.CreatedById)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.tasks1)
                .WithOptional(e => e.user1)
                .HasForeignKey(e => e.AssignedUserId);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.taskgroups)
                .WithRequired(e => e.user)
                .HasForeignKey(e => e.CreatedById)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.taskinstances)
                .WithRequired(e => e.user)
                .HasForeignKey(e => e.ExecutedById)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.transactionanalysiscomments)
                .WithRequired(e => e.user)
                .HasForeignKey(e => e.CreatedBy)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.uploads)
                .WithRequired(e => e.user)
                .HasForeignKey(e => e.CreatedById)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.uploadformats)
                .WithRequired(e => e.user)
                .HasForeignKey(e => e.CreatedById)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<task>()
                .HasMany(e => e.financialcontrolreportdefinitions)
                .WithRequired(e => e.tasks)
                .HasForeignKey(k => k.TaskId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.financialcontrolreportdefinitions)
                .WithRequired(e => e.users)
                .HasForeignKey(k => k.CreatedBy)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<Account>()
                .HasMany(e => e.financialcontrolbalanceaccounts)
                .WithRequired(e => e.Accounts)
                .HasForeignKey(k => k.AccountId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<financialcontrolreportdefinition>()
                .HasMany(e => e.financialcontrolbalanceaccounts)
                .WithRequired(e => e.financialcontrolreportdefinitions)
                .HasForeignKey(k => k.FinancialControlReportDefinitionId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<financialcontrolreportdefinition>()
                .HasMany(e => e.financialcontroltotalprofiles)
                .WithRequired(e => e.financialcontrolreportdefinitions)
                .HasForeignKey(k => k.FinancialControlReportDefinitionId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<financialcontrolreportdefinition>()
                .HasMany(e => e.financialcontrolrecenttransactionaccounts)
                .WithRequired(e => e.financialcontrolreportdefinitions)
                .HasForeignKey(k => k.FinancialControlReportDefinitionId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<financialcontrolreportdefinition>()
                .HasMany(e => e.financialcontrolprofiletrends)
                .WithRequired(e => e.financialcontrolreportdefinitions)
                .HasForeignKey(k => k.FinancialControlReportDefinitionId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<financialcontrolreportdefinition>()
                .HasMany(e => e.financialcontrolmanualbalances)
                .WithRequired(e => e.financialcontrolreportdefinitions)
                .HasForeignKey(k => k.FinancialControlReportDefinitionId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<financialcontrolreportdefinition>()
                .HasMany(e => e.financialcontrolaccounttrends)
                .WithRequired(e => e.financialcontrolreportdefinitions)
                .HasForeignKey(k => k.FinancialControlReportDefinitionId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<financialcontrolreportdefinition>()
                .HasMany(e => e.fcreportexecutioninstances)
                .WithRequired(e => e.financialcontrolreportdefinitions)
                .HasForeignKey(k => k.FinancialControlReportDefinitionId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<financialcontrolratio>()
                .HasMany(e => e.fcratioaccounts)
                .WithRequired(e => e.financialcontrolratios)
                .HasForeignKey(k => k.FinancialControlRatioId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<financialcontrolreportdefinition>()
                .HasMany(e => e.financialcontrolratios)
                .WithRequired(e => e.financialcontrolreportdefinitions)
                .HasForeignKey(k => k.FinancialControlReportDefinitionId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<financialcontrolprofiletrend>()
                .HasMany(e => e.financialcontroltrendprofiletaskxrefs)
                .WithRequired(e => e.financialcontrolprofiletrends)
                .HasForeignKey(k => k.FinancialControlProfileTrendId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<task>()
                .HasMany(e => e.financialcontroltrendprofiletaskxrefs)
                .WithRequired(e => e.tasks)
                .HasForeignKey(k => k.TaskId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<task>()
                .HasMany(e => e.financialcontrolreportdefinitions)
                .WithRequired(e => e.tasks)
                .HasForeignKey(k => k.TaskId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<Account>()
                .HasMany(e => e.financialcontrolrecenttransactionaccounts)
                .WithRequired(e => e.Accounts)
                .HasForeignKey(k => k.AccountId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<manualbalance>()
                .HasMany(e => e.financialcontrolmanualbalances)
                .WithRequired(e => e.manualbalances)
                .HasForeignKey(k => k.ManualBalanceId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<manualbalance>()
                .HasMany(e => e.fcratiomanualbalances)
                .WithRequired(e => e.manualbalances)
                .HasForeignKey(k => k.ManualBalanceId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<Account>()
                .HasMany(e => e.financialcontrolaccounttrends)
                .WithRequired(e => e.Accounts)
                .HasForeignKey(k => k.AccountId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<taskinstance>()
                .HasMany(e => e.finacialcontroltasks)
                .WithRequired(e => e.taskinstances)
                .HasForeignKey(k => k.TaskInstanceId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<financialcontroltotalprofile>()
                .HasMany(e => e.fctotalprofiletotals)
                .WithRequired(e => e.financialcontroltotalprofiles)
                .HasForeignKey(k => k.FinancialControlBalanceProfileId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<fcreportexecutioninstance>()
                .HasMany(e => e.fctotalprofiletotals)
                .WithRequired(e => e.fcreportexecutioninstances)
                .HasForeignKey(k => k.FcReportExecutionInstanceId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<task>()
                .HasMany(e => e.fctotalprofiletaskxrefs)
                .WithRequired(e => e.tasks)
                .HasForeignKey(k => k.TaskId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<financialcontroltotalprofile>()
                .HasMany(e => e.fctotalprofiletaskxrefs)
                .WithRequired(e => e.financialcontroltotalprofiles)
                .HasForeignKey(k => k.FinancialControlTotalProfileId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<financialcontrolmanualbalance>()
                .HasMany(e => e.fctotalmanualbalancevalues)
                .WithRequired(e => e.financialcontrolmanualbalances)
                .HasForeignKey(k => k.FinancialControlManualBalancesId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<fcreportexecutioninstance>()
                .HasMany(e => e.fctotalmanualbalancevalues)
                .WithRequired(e => e.fcreportexecutioninstances)
                .HasForeignKey(k => k.FcReportExecutionInstanceId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<financialcontrolbalanceaccount>()
                .HasMany(e => e.fctotalaccountbalances)
                .WithRequired(e => e.financialcontrolbalanceaccounts)
                .HasForeignKey(k => k.FinancialControlBalanceAccountId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<fcreportexecutioninstance>()
                .HasMany(e => e.fctotalaccountbalances)
                .WithRequired(e => e.fcreportexecutioninstances)
                .HasForeignKey(k => k.FcReportExecutionInstanceId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<financialcontrolrecenttransactionaccounts>()
                .HasMany(e => e.fcreporttransactionxrefs)
                .WithRequired(e => e.financialcontrolrecenttransactionaccount)
                .HasForeignKey(k => k.FinancialControlRecentTransactionAccountsId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<transaction>()
                .HasMany(e => e.fcreporttransactionxrefs)
                .WithRequired(e => e.transactions)
                .HasForeignKey(k => k.TransactionId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<fcreportexecutioninstance>()
                .HasMany(e => e.fcratioprofilevalues)
                .WithRequired(e => e.fcreportexecutioninstances)
                .HasForeignKey(k => k.FcReportExecutionInstanceId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<fcratioprofile>()
                .HasMany(e => e.fcratioprofilevalues)
                .WithRequired(e => e.fcratioprofiles)
                .HasForeignKey(k => k.FcRatioProfileId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<fcratioprofile>()
                .HasMany(e => e.fcratioprofiletaskxrefs)
                .WithRequired(e => e.fcratioprofiles)
                .HasForeignKey(k => k.FcRatioProfileId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<task>()
                .HasMany(e => e.fcratioprofiletaskxrefs)
                .WithRequired(e => e.tasks)
                .HasForeignKey(k => k.TaskId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<financialcontrolratio>()
                .HasMany(e => e.fcratioprofiles)
                .WithRequired(e => e.financialcontrolratios)
                .HasForeignKey(k => k.FinancialControlRatioId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<fcratiomanualbalance>()
                .HasMany(e => e.fcratiomanualbalancevalues)
                .WithRequired(e => e.fcratiomanualbalances)
                .HasForeignKey(k => k.FcRatioManualBalanceId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<fcreportexecutioninstance>()
                .HasMany(e => e.fcratiomanualbalancevalues)
                .WithRequired(e => e.fcreportexecutioninstances)
                .HasForeignKey(k => k.FcReportExecutionInstanceId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<financialcontrolratio>()
                .HasMany(e => e.fcratiomanualbalances)
                .WithRequired(e => e.financialcontrolratios)
                .HasForeignKey(k => k.FinancialControlRatioId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<manualbalance>()
                .HasMany(e => e.fcratiomanualbalances)
                .WithRequired(e => e.manualbalances)
                .HasForeignKey(k => k.ManualBalanceId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<fcratioaccount>()
                .HasMany(e => e.fcratioaccountvalues)
                .WithRequired(e => e.fcratioaccounts)
                .HasForeignKey(k => k.FcRatioAccountId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<fcreportexecutioninstance>()
                .HasMany(e => e.fcratioaccountvalues)
                .WithRequired(e => e.fcreportexecutioninstances)
                .HasForeignKey(k => k.FcReportExecutionInstanceId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<fcreportexecutioninstance>()
                .HasMany(e => e.fcprofiletrendvaluess)
                .WithRequired(e => e.fcreportexecutioninstances)
                .HasForeignKey(k => k.FcReportExecutionInstanceId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<financialcontrolprofiletrend>()
                .HasMany(e => e.fcprofiletrendvalue)
                .WithRequired(e => e.financialcontrolprofiletrends)
                .HasForeignKey(k => k.FinancialControlProfileTrendId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<Account>()
                .HasMany(e => e.fcratioaccounts)
                .WithRequired(e => e.Accounts)
                .HasForeignKey(k => k.AccountId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<fcreportexecutioninstance>()
                .HasMany(e => e.fcaccounttrendvaluess)
                .WithRequired(e => e.fcreportexecutioninstances)
                .HasForeignKey(k => k.FcReportExecutionInstanceId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<financialcontrolaccounttrend>()
                .HasMany(e => e.fcaccounttrendvalue)
                .WithRequired(e => e.financialcontrolaccounttrends)
                .HasForeignKey(k => k.FinancialControlAccountTrendId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<task>()
                .HasMany(e => e.transactionmatchingrules)
                .WithRequired(e => e.tasks)
                .HasForeignKey(k => k.TaskId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<task>()
                .HasMany(e => e.transactionmatchingtaskrulesaccess)
                .WithRequired(e => e.tasks)
                .HasForeignKey(k => k.TaskId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<transactionmatchingtaskrule>()
                .HasMany(e => e.tmtaskalerteexref)
                .WithRequired(e => e.transactionmatchingtaskrule)
                .HasForeignKey(k => k.TransactionMatchingTaskRuleId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<task>()
                .HasMany(e => e.transactionmatchingtaskrules)
                .WithRequired(e => e.task)
                .HasForeignKey(k => k.TaskId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.transactionmatchingtaskrules)
                .WithRequired(e => e.user)
                .HasForeignKey(k => k.CreatedBy)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.tmtaskalerteexrefs)
                .WithRequired(e => e.user)
                .HasForeignKey(k => k.UsersId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.manualbalances)
                .WithRequired(e => e.users)
                .HasForeignKey(k => k.LastUpdatedBy)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<fcautoexecute>()
                .HasMany(e => e.financialcontrolreportdefinitions)
                .WithRequired(e => e.fcautoexecutes)
                .HasForeignKey(k => k.IdAutoExecute)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.singleaccountalerts)
                .WithRequired(e => e.user)
                .HasForeignKey(k => k.CreatedBy)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.singleaccountalertusersxrefs)
                .WithRequired(e => e.user)
                .HasForeignKey(k => k.UsersId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<task>()
                .HasMany(e => e.singleaccountalerttransanalysisxrefs)
                .WithRequired(e => e.tasks)
                .HasForeignKey(k => k.TaskId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<Account>()
                .HasMany(e => e.singleaccountalerts)
                .WithRequired(e => e.Accounts)
                .HasForeignKey(k => k.AccountId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<alertcondition>()
                .HasMany(e => e.singleaccountalerts)
                .WithRequired(e => e.alertconditions)
                .HasForeignKey(k => k.AlertConditionId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<singleaccountalert>()
                .HasMany(e => e.singleaccountalerttransanalysisxrefs)
                .WithRequired(e => e.singleaccountalerts)
                .HasForeignKey(k => k.SingleAccountAlertId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<singleaccountalert>()
                .HasMany(e => e.singleaccountalertusersxrefs)
                .WithRequired(e => e.singleaccountalerts)
                .HasForeignKey(k => k.SingleAccountAlertId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.multipleaccountalerts)
                .WithRequired(e => e.user)
                .HasForeignKey(k => k.CreatedBy)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.multipleaccountalertuserxrefs)
                .WithRequired(e => e.user)
                .HasForeignKey(k => k.UsersId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<task>()
                .HasMany(e => e.alertmultipleprofiletaskxrefs)
                .WithRequired(e => e.tasks)
                .HasForeignKey(k => k.TaskId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<Account>()
                .HasMany(e => e.alertmultipleaccounts)
                .WithRequired(e => e.Accounts)
                .HasForeignKey(k => k.AccountId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<multipleaccountalert>()
                .HasMany(e => e.alertmultipleaccounts)
                .WithRequired(e => e.multipleaccountalerts)
                .HasForeignKey(k => k.MultipleAccountAlertId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<multipleaccountalert>()
                .HasMany(e => e.alertmultipleprofiles)
                .WithRequired(e => e.multipleaccountalerts)
                .HasForeignKey(k => k.MultipleAccountAlertId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<multipleaccountalert>()
                .HasMany(e => e.multipleaccountalertuserxrefs)
                .WithRequired(e => e.multipleaccountalerts)
                .HasForeignKey(k => k.MultipleAccountAlertId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<alertmultipleprofile>()
                .HasMany(e => e.alertmultipleprofiletaskxrefs)
                .WithRequired(e => e.alertmultipleprofiles)
                .HasForeignKey(k => k.AlertMultipleProfileId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<taskinstance>()
                .HasMany(e => e.balanceanalysistasks)
                .WithRequired(e => e.taskinstances)
                .HasForeignKey(k => k.TaskInstanceId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<task>()
                .HasMany(e => e.balanceanalysismappingrules)
                .WithRequired(e => e.tasks)
                .HasForeignKey(k => k.TaskId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<task>()
                .HasMany(e => e.balanceanalysisactions)
                .WithRequired(e => e.tasks)
                .HasForeignKey(k => k.TaskId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<balanceanalysistask>()
                .HasMany(e => e.balanceanalysisdatasets)
                .WithRequired(e => e.balanceanalysistasks)
                .HasForeignKey(k => k.BalanceAnalysisTaskId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<balanceanalysisdataset>()
                .HasMany(e => e.balanceanalysismatch1s)
                .WithRequired(e => e.balanceanalysisdataset1)
                .HasForeignKey(k => k.DataSet1Id)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<balanceanalysisdataset>()
                .HasMany(e => e.balanceanalysismatch2s)
                .WithRequired(e => e.balanceanalysisdataset2)
                .HasForeignKey(k => k.DataSet2Id)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<balanceanalysisaction>()
                .HasMany(e => e.balanceanalysismatchs)
                .WithRequired(e => e.balanceanalysisactions)
                .HasForeignKey(k => k.BalanceAnalysisActionId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<balanceanalysismappingrule>()
                .HasMany(e => e.balanceanalysismatchs)
                .WithRequired(e => e.balanceanalysismappingrules)
                .HasForeignKey(k => k.balanceanalysismappingruleId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<balanceanalysistask>()
                .HasMany(e => e.balanceanalysiscomments)
                .WithRequired(e => e.balanceanalysistasks)
                .HasForeignKey(k => k.BalanceAnalysisTaskId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.balanceanalysiscomments)
                .WithRequired(e => e.users)
                .HasForeignKey(k => k.CreatedBy)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.balanceanalysisdocuments)
                .WithRequired(e => e.users)
                .HasForeignKey(k => k.CreatedBy)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<balanceanalysistask>()
                .HasMany(e => e.balanceanalysisdocuments)
                .WithRequired(e => e.balanceanalysistasks)
                .HasForeignKey(k => k.BalanceAnalysisTaskId)
                .WillCascadeOnDelete(false);

            #endregion Cleanbooks

            #region Qbicles

            modelBuilder.Entity<QbicleDomain>()
                .HasMany(x => x.AvailableApps)
                .WithMany(x => x.DomainsAvailable)
                .Map(x =>
                {
                    x.ToTable("qb_DomainAvailableAppsXref");
                    x.MapLeftKey("Domain_Id");
                    x.MapRightKey("App_Id");
                });

            modelBuilder.Entity<QbicleDomain>()
                .HasMany(x => x.SubscribedApps)
                .WithMany(x => x.DomainsSubscribed)
                .Map(x =>
                {
                    x.ToTable("qb_DomainsSubscribedAppsXref");
                    x.MapLeftKey("Domain_Id");
                    x.MapRightKey("App_Id");
                });

            modelBuilder.Entity<QbicleTask>()
                .HasMany(x => x.FormDefinitions)
                .WithMany(x => x.Tasks)
                .Map(x =>
                {
                    x.ToTable("qb_TaskFormDefinitionXref");
                    x.MapLeftKey("Task_Id");
                    x.MapRightKey("FormDefinition_Id");
                });

            modelBuilder.Entity<Qbicle>()
                .HasMany(x => x.Members)
                .WithMany(x => x.Qbicles)
                .Map(x =>
                {
                    x.ToTable("qb_QbiclesUsersXref");
                    x.MapLeftKey("Qbicle_Id");
                    x.MapRightKey("User_Id");
                });

            modelBuilder.Entity<QbicleDomain>()
                .HasMany(x => x.Users)
                .WithMany(x => x.Domains)
                .Map(x =>
                {
                    x.ToTable("qb_DomainsUsersXref");
                    x.MapLeftKey("Domain_Id");
                    x.MapRightKey("User_Id");
                });

            modelBuilder.Entity<QbicleDomain>()
                .HasMany(x => x.QbicleCashiers)
                .WithMany(x => x.QbicleCashiersByDomain)
                .Map(x =>
                {
                    x.ToTable("qb_qbiclecashiersbydomainxref");
                    x.MapLeftKey("Domain_Id");
                    x.MapRightKey("User_Id");
                });

            modelBuilder.Entity<QbicleDomain>()
              .HasMany(x => x.QbicleSupervisors)
              .WithMany(x => x.QbicleSupervisorsByDomain)
              .Map(x =>
              {
                  x.ToTable("qb_qbiclsupervisorsbydomainxref");
                  x.MapLeftKey("Domain_Id");
                  x.MapRightKey("User_Id");
              });

            modelBuilder.Entity<QbicleDomain>()
              .HasMany(x => x.QbicleManagers)
              .WithMany(x => x.QbicleManagersByDomain)
              .Map(x =>
              {
                  x.ToTable("qb_QbicleManagersByDomainXref");
                  x.MapLeftKey("Domain_Id");
                  x.MapRightKey("User_Id");
              });

            modelBuilder.Entity<SubscriptionAccount>()
                .HasMany(x => x.Users)
                .WithMany(x => x.AccountUsers)
                .Map(x =>
                    {
                        x.ToTable("qb_AccountUsersXref");
                        x.MapLeftKey("Account_Id");
                        x.MapRightKey("User_Id");
                    }
                );

            modelBuilder.Entity<SubscriptionAccount>()
                .HasMany(x => x.Administrators)
                .WithMany(x => x.AccountAdministrators)
                .Map(x =>
                {
                    x.ToTable("qb_AccountAdministratorsXref");
                    x.MapLeftKey("Account_Id");
                    x.MapRightKey("User_Id");
                });

            modelBuilder.Entity<QbicleDomain>()
                .HasMany(x => x.Administrators)
                .WithMany(x => x.DomainAdministrators)
                .Map(x =>
                    {
                        x.ToTable("qb_DomainAdministratorsXref");
                        x.MapLeftKey("Domain_Id");
                        x.MapRightKey("User_Id");
                    }
                );

            modelBuilder.Entity<QbicleActivity>()
                .HasMany(x => x.ActivityMembers)
                .WithMany(x => x.Activities)
                .Map(x =>
                {
                    x.ToTable("qb_ActivitiesUsersXref");
                    x.MapLeftKey("Activity_Id");
                    x.MapRightKey("User_Id");
                });

            modelBuilder.Entity<MyTag>()
                .HasMany(x => x.Activities)
                .WithMany(x => x.Folders)
                .Map(x =>
                {
                    x.ToTable("qb_TagsActivitiesXref");

                    x.MapLeftKey("Folder_Id");
                    x.MapRightKey("Activity_Id");
                });

            modelBuilder.Entity<MyTag>()
                .HasMany(x => x.Posts)
                .WithMany(x => x.Folders)
                .Map(x =>
                {
                    x.ToTable("qb_TagsPostsXref");
                    x.MapLeftKey("Folder_Id");
                    x.MapRightKey("QbiclePost_Id");
                });

            modelBuilder.Entity<PosDeviceType>()
                .HasMany(x => x.PosOrderTypes)
                .WithMany(x => x.PosDeviceTypes)
                .Map(x =>
                {
                    x.ToTable("posordertypeposdevicetypes");
                    x.MapLeftKey("PosDeviceType_Id");
                    x.MapRightKey("PosOrderType_Id");
                });

            //modelBuilder.Entity<ApplicationUser>()
            //  .HasMany(e => e.QbiclesManaged)
            //  .WithRequired(e => e.Cashier)
            //  .WillCascadeOnDelete(false);

            //modelBuilder.Entity<ApplicationUser>()
            //  .HasMany(e => e.QbiclesManaged)
            //  .WithRequired(e => e.Supervisor)
            //  .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
              .HasMany(e => e.QbiclesManaged)
              .WithRequired(e => e.Manager)
              .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.DomainRoles)
                .WithMany(x => x.Users)
                .Map(x =>
                {
                    x.ToTable("qb_UserRoleXref");
                    x.MapLeftKey("User_Id");
                    x.MapRightKey("DomainRole_Id");
                });

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.Reviewers)
                .WithMany(x => x.ReviewedBy)
                .Map(
                    x =>
                    {
                        x.ToTable("qb_ApprovalAppReviewersXref");
                        x.MapLeftKey("User_Id");
                        x.MapRightKey("ApprovalReq_Id");
                    });

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.ApprovalInitiators)
                .WithMany(x => x.Initiators)
                .Map(
                    x =>
                    {
                        x.ToTable("qb_ApprovalRequestDefinitionInitiatorsXref");
                        x.MapLeftKey("User_Id");
                        x.MapRightKey("ApprovalRequestDefinition_Id");
                    });

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.ApprovalReviewers)
                .WithMany(x => x.Reviewers)
                .Map(
                    x =>
                    {
                        x.ToTable("qb_ApprovalRequestDefinitionReviewersXref");
                        x.MapLeftKey("User_Id");
                        x.MapRightKey("ApprovalRequestDefinition_Id");
                    });

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.ApprovalApprovers)
                .WithMany(x => x.Approvers)
                .Map(
                    x =>
                    {
                        x.ToTable("qb_ApprovalRequestDefinitionApproversXref");
                        x.MapLeftKey("User_Id");
                        x.MapRightKey("ApprovalRequestDefinition_Id");
                    });

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.Interests)
                .WithMany(x => x.Users)
                .Map(x =>
                {
                    x.ToTable("qb_BusinessCategoriesUsersXref");
                    x.MapLeftKey("User_Id");
                    x.MapRightKey("BusinessCategory_Id");
                });

            modelBuilder.Entity<WaitListRequest>()
                .HasMany(x => x.BusinessCategories)
                .WithMany(x => x.WaitListRequests)
                .Map(x =>
                {
                    x.ToTable("qb_WaitListRequestCategoriesUsersXref");
                    x.MapLeftKey("WaitListRequest_Id");
                    x.MapRightKey("BusinessCategory_Id");
                });

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.RemovedQbicle)
                .WithMany(x => x.RemovedForUsers)
                .Map(x =>
                {
                    x.ToTable("qb_RemovedQbicleUserXref");
                    x.MapLeftKey("User_Id");
                    x.MapRightKey("Qbicle_Id");
                });
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.NotViewedQbicle)
                .WithMany(x => x.NotViewedBy)
                .Map(x =>
                {
                    x.ToTable("qb_NotViewedQbicleUserXref");
                    x.MapLeftKey("User_Id");
                    x.MapRightKey("C2CQbicle_Id");
                });

            //Qbicle root V2
            modelBuilder.Entity<QbicleTask>()
               .HasMany(e => e.QSteps)
               .WithRequired(e => e.Task)
               .HasForeignKey(e => e.ActivityId)
               .WillCascadeOnDelete(false);

            modelBuilder.Entity<QbicleTask>()
                .HasMany(e => e.QTimeSpents)
                .WithRequired(e => e.Task)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<QbicleTask>()
                .HasMany(e => e.QPerformances)
                .WithRequired(e => e.Task)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<QbicleTask>()
                .HasMany(e => e.QStepinstances)
                .WithRequired(e => e.Task)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.Performances)
                .WithRequired(e => e.User)
                .HasForeignKey(e => e.RatedBy)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.Peoples)
                .WithRequired(e => e.User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<QbicleActivity>()
                .HasMany(e => e.Jobs)
                .WithRequired(e => e.Activity)
                .HasForeignKey(e => e.ActivityId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<QbicleActivity>()
                .HasMany(e => e.Relateds)
                .WithRequired(e => e.Activity)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<QbicleJob>()
                .Property(e => e.JobId)
                .IsUnicode(false);

            modelBuilder.Entity<QbicleJob>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<QbiclePerformance>()
                .Property(e => e.RatedDateTime)
                .HasPrecision(0);

            modelBuilder.Entity<QbicleRecurrance>()
                .Property(e => e.Days)
                .IsUnicode(false);

            modelBuilder.Entity<QbicleRecurrance>()
                .Property(e => e.Months)
                .IsUnicode(false);

            modelBuilder.Entity<QbicleStep>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<QbicleStep>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<QbicleStep>()
                .HasMany(e => e.StepInstance)
                .WithRequired(e => e.Step)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<QbicleSet>()
                .HasOptional(e => e.Recurrance)
                .WithRequired(e => e.AssociatedSet);

            modelBuilder.Entity<QbicleSet>()
                .HasMany(e => e.Peoples)
                .WithRequired(e => e.AssociatedSet)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<QbicleSet>()
                .HasMany(e => e.Activities)
                .WithOptional(e => e.AssociatedSet);

            modelBuilder.Entity<QbicleSet>()
                .HasMany(e => e.Relateds)
                .WithRequired(e => e.AssociatedSet)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<QbicleSet>()
                .HasMany(e => e.QbiclePosts)
                .WithOptional(e => e.Set);

            modelBuilder.Entity<Qbicles.Models.Invitation.Invitation>()
                .HasMany(e => e.Log)
                .WithRequired(e => e.Invitation)
                .WillCascadeOnDelete(false);
            //end

            #endregion Qbicles

            #region Bookkeeping

            modelBuilder.Entity<BKTransaction>()
                .HasMany(x => x.Dimensions)
                .WithMany(x => x.DefaultDimensionTransactions)
                .Map(x =>
                {
                    x.ToTable("bk_dimensiontransactions");
                    x.MapLeftKey("TransactionDimension_Id");
                    x.MapRightKey("BKTransaction_Id");
                });

            // Explicitly implement the cross references bewteen the users table and the bookkeeping workgroup members, Reviewers and Approvers
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.BKWorkGroupMembers)
                .WithMany(x => x.Members)
                .Map(
                    x =>
                    {
                        x.ToTable("bk_workgroupmembersxref");
                        x.MapLeftKey("User_Id");
                        x.MapRightKey("WorkGroup_Id");
                    });
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.BKWorkGroupReviewers)
                .WithMany(x => x.Reviewers)
                .Map(
                    x =>
                    {
                        x.ToTable("bk_workgroupreviewersxref");
                        x.MapLeftKey("User_Id");
                        x.MapRightKey("WorkGroup_Id");
                    });
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.BKWorkGroupApprovers)
                .WithMany(x => x.Approvers)
                .Map(
                    x =>
                    {
                        x.ToTable("bk_workgroupapproversxref");
                        x.MapLeftKey("User_Id");
                        x.MapRightKey("WorkGroup_Id");
                    });
            // Explicitly implement the cross references bewteen the users table and the cleanbook workgroup members, Reviewers and Approvers
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.CBWorkGroupMembers)
                .WithMany(x => x.Members)
                .Map(
                    x =>
                    {
                        x.ToTable("cb_workgroupmembersxref");
                        x.MapLeftKey("User_Id");
                        x.MapRightKey("WorkGroup_Id");
                    });
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.CBWorkGroupReviewers)
                .WithMany(x => x.Reviewers)
                .Map(
                    x =>
                    {
                        x.ToTable("cb_workgroupreviewersxref");
                        x.MapLeftKey("User_Id");
                        x.MapRightKey("WorkGroup_Id");
                    });
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.CBWorkGroupApprovers)
                .WithMany(x => x.Approvers)
                .Map(
                    x =>
                    {
                        x.ToTable("cb_workgroupapproversxref");
                        x.MapLeftKey("User_Id");
                        x.MapRightKey("WorkGroup_Id");
                    });

            #endregion Bookkeeping

            #region Community

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.FollowedCommunityPages)
                .WithMany(x => x.Followers)
                .Map(
                    x =>
                    {
                        x.ToTable("com_communitypage_follower_xref");
                        x.MapLeftKey("User_Id");
                        x.MapRightKey("Id");
                    });

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.FollowedDomainProfiles)
                .WithMany(x => x.Followers)
                .Map(
                    x =>
                    {
                        x.ToTable("com_domainprofilepage_follower_xref");
                        x.MapLeftKey("User_Id");
                        x.MapRightKey("Id");
                    });

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.FollowedUserProfilePages)
                .WithMany(x => x.Followers)
                .Map(
                    x =>
                    {
                        x.ToTable("com_userprofilepage_follower_xref");
                        x.MapLeftKey("User_Id");
                        x.MapRightKey("Id");
                    });

            modelBuilder.Entity<QbicleDomain>()
                .HasMany(x => x.HighlightPosterHiddenUser)
                .WithMany(x => x.HighlightDomainHidden)
                .Map(x =>
                {
                    x.ToTable("highlightdomain_hidden_user_xref");
                    x.MapLeftKey("Hidden_Domain_Id");
                    x.MapRightKey("User_Id");
                });

            modelBuilder.Entity<HighlightPost>()
                .HasMany(x => x.LikedBy)
                .WithMany(x => x.LikedHighlightPosts)
                .Map(x =>
                {
                    x.ToTable("likedhighlight_user_xref");
                    x.MapLeftKey("Highlight_Id");
                    x.MapRightKey("User_Id");
                });

            modelBuilder.Entity<HighlightPost>()
                .HasMany(x => x.BookmarkedBy)
                .WithMany(x => x.BookmarkedHighlightPosts)
                .Map(x =>
                {
                    x.ToTable("bookmarkedhighlight_user_xref");
                    x.MapLeftKey("Highlight_Id");
                    x.MapRightKey("User_Id");
                });

            modelBuilder.Entity<ListingHighlight>()
                .HasMany(x => x.FlaggedBy)
                .WithMany(x => x.FlaggedListings)
                .Map(x =>
                {
                    x.ToTable("flaggedlisting_user_xref");
                    x.MapLeftKey("Highlight_Id");
                    x.MapRightKey("User_Id");
                });

            modelBuilder.Entity<ShortListGroup>()
                .HasMany(x => x.Candidates)
                .WithMany(x => x.AssociatedShortListGroups)
                .Map(x =>
                {
                    x.ToTable("shortlist_user_xref");
                    x.MapLeftKey("Shortlist_Id");
                    x.MapRightKey("User_Id");
                });

            modelBuilder.Entity<HighlightPost>()
                .HasMany(x => x.Tags)
                .WithMany(x => x.HLPosts)
                .Map(x =>
                {
                    x.ToTable("hlpost_tag_xref");
                    x.MapLeftKey("HlPost_Id");
                    x.MapRightKey("Tag_Id");
                });

            #endregion Community

            #region Trader

            modelBuilder.Entity<WorkGroup>()
                .HasMany(x => x.Processes)
                .WithMany(x => x.WorkGroups)
                .Map(
                    x =>
                    {
                        x.ToTable("trad_traderprocessworkgroups_xref");
                        x.MapLeftKey("WorkGroup_Id");
                        x.MapRightKey("TraderProcess_Id");
                    });
            modelBuilder.Entity<TransactionItemLog>()
                .HasMany(x => x.Dimensions)
                .WithMany(x => x.TransactionItemLogs)
                .Map(
                    x =>
                    {
                        x.ToTable("trad_TransactionItemLogDimensions_xref");
                        x.MapLeftKey("TransactionItemLog_Id");
                        x.MapRightKey("Dimension_Id");
                    });
            modelBuilder.Entity<TraderTransactionItem>()
                .HasMany(x => x.Dimensions)
                .WithMany(x => x.TraderTransactionItems)
                .Map(
                    x =>
                    {
                        x.ToTable("trad_TraderTransactionItemDimensions_xref");
                        x.MapLeftKey("TraderTransactionItem_Id");
                        x.MapRightKey("Dimension_Id");
                    });
            // Explicitly implement the cross reference between TraderLocation and TraderItem
            modelBuilder.Entity<TraderLocation>()
                .HasMany(x => x.Items)
                .WithMany(x => x.Locations)
                .Map(
                    x =>
                    {
                        x.ToTable("trad_location_item_xref");
                        x.MapLeftKey("Location_Id");
                        x.MapRightKey("Item_Id");
                    });
            //trader
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.WorkGroupMembers)
                .WithMany(x => x.Members)
                .Map(
                    x =>
                    {
                        x.ToTable("trad_workgroupmembersxref");
                        x.MapLeftKey("User_Id");
                        x.MapRightKey("WorkGroup_Id");
                    });
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.WorkGroupReviewers)
                .WithMany(x => x.Reviewers)
                .Map(
                    x =>
                    {
                        x.ToTable("trad_workgroupreviewersxref");
                        x.MapLeftKey("User_Id");
                        x.MapRightKey("WorkGroup_Id");
                    });
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.WorkGroupApprovers)
                .WithMany(x => x.Approvers)
                .Map(
                    x =>
                    {
                        x.ToTable("trad_workgroupapproversxref");
                        x.MapLeftKey("User_Id");
                        x.MapRightKey("WorkGroup_Id");
                    });

            modelBuilder.Entity<TraderGroup>()
                .HasMany(x => x.WorkGroupCategories)
                .WithMany(x => x.ItemCategories)
                .Map(
                    x =>
                    {
                        x.ToTable("trad_workgroupitemcategoriesxref");
                        x.MapLeftKey("Group_Id");
                        x.MapRightKey("WorkGroup_Id");
                    });

            // QBIC-1437:  modelBuilder.Entity<PrepDisplayDevice>()
            //    .HasMany(x => x.Administrators)
            //    .WithMany(x => x.AdminForPrepDisplayDevice)
            //    .Map(
            //        x =>
            //        {
            //            x.ToTable("ods_adminforprepdisplaydevice");
            //            x.MapLeftKey("PrepDisplayDevice_Id");
            //            x.MapRightKey("AdminForPrepDisplayDevice_Id");
            //        });

            modelBuilder.Entity<TraderItem>()
                .HasMany(x => x.TaxRates)
                .WithMany(x => x.TraderItems)
                .Map(x =>
                {
                    x.ToTable("trad_TaxRatesTraderItemsXref");

                    x.MapLeftKey("TraderItem_Id");
                    x.MapRightKey("TaxRate_Id");
                });

            modelBuilder.Entity<Catalog>()
                .HasMany(x => x.OrderItemDimensions)
                .WithMany(x => x.PosMenus)
                .Map(x =>
                {
                    x.ToTable("posmenutransactiondimensions");
                    x.MapLeftKey("PosMenu_Id");
                    x.MapRightKey("TransactionDimension_Id");
                });

            modelBuilder.Entity<TraderReference>()
                .HasIndex(p => new { p.FullRef, p.Type, p.Domain_Id }).IsUnique();
            modelBuilder.Entity<JournalEntry>()
                .HasIndex(p => new { p.Number, p.Domain_Id }).IsUnique();

            modelBuilder.Entity<ReorderItem>()
                .HasMany(x => x.Dimensions)
                .WithMany(x => x.ReorderItems)
                .Map(x =>
                {
                    x.ToTable("trad_DimensionsReorderItemsXref");

                    x.MapLeftKey("ReorderItem_Id");
                    x.MapRightKey("Dimension_Id");
                });

            modelBuilder.Entity<PosOrderType>()
                .HasMany(x => x.OdsDeviceTypes)
                .WithMany(x => x.AssociatedOrderTypes)
                .Map(x =>
                {
                    x.ToTable("odsdevicetypeposordertypes");

                    x.MapLeftKey("PosOrderType_Id");
                    x.MapRightKey("OdsDeviceType_Id");
                });
            modelBuilder.Entity<PosOrderType>()
               .HasMany(x => x.PosDeviceTypes)
               .WithMany(x => x.PosOrderTypes)
               .Map(x =>
               {
                   x.ToTable("posordertypeposdevicetypes");

                   x.MapLeftKey("PosDeviceType_Id");
                   x.MapRightKey("PosOrderType_Id");
               });

            #endregion Trader

            #region DDS

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.AdminForDdsDevice)
                .WithMany(x => x.Administrators)
                .Map(
                    x =>
                    {
                        x.ToTable("dds_deviceadministrator");
                        x.MapLeftKey("User_Id");
                        x.MapRightKey("ddsdevice_Id");
                    });

            modelBuilder.Entity<DeviceUser>()
                .HasMany(x => x.Devices)
                .WithMany(x => x.Users)
                .Map(
                    x =>
                    {
                        x.ToTable("posuser_pos_devices");
                        x.MapLeftKey("PosUser_Id");
                        x.MapRightKey("PosDevice_Id");
                    });

            modelBuilder.Entity<PosCashier>()
                .HasMany(x => x.Devices)
                .WithMany(x => x.DeviceCashiers)
                .Map(
                    x =>
                    {
                        x.ToTable("pos_cashier_pos_devices");
                        x.MapLeftKey("PosCashier_Id");
                        x.MapRightKey("PosDevice_Id");
                    });

            modelBuilder.Entity<PosSupervisor>()
                .HasMany(x => x.Devices)
                .WithMany(x => x.DeviceSupervisors)
                .Map(
                    x =>
                    {
                        x.ToTable("pos_supervisor_pos_devices");
                        x.MapLeftKey("PosSupervisor_Id");
                        x.MapRightKey("PosDevice_Id");
                    });

            modelBuilder.Entity<PosTillManager>()
                .HasMany(x => x.Devices)
                .WithMany(x => x.DeviceManagers)
                .Map(
                    x =>
                    {
                        x.ToTable("pos_tillmanager_pos_devices");
                        x.MapLeftKey("PosTillManager_Id");
                        x.MapRightKey("PosDevice_Id");
                    });

            modelBuilder.Entity<DeviceUser>()
                .HasMany(x => x.DdsDevices)
                .WithMany(x => x.Users)
                .Map(
                    x =>
                    {
                        x.ToTable("posuser_dds_devices");
                        x.MapLeftKey("PosUser_Id");
                        x.MapRightKey("DdsDevice_Id");
                    });

            modelBuilder.Entity<DeviceUser>()
                .HasMany(x => x.PrepDisplayDevices)
                .WithMany(x => x.Users)
                .Map(
                    x =>
                    {
                        x.ToTable("posuser_pds_devices");
                        x.MapLeftKey("PosUser_Id");
                        x.MapRightKey("PrepDisplayDevices_Id");
                    });

            modelBuilder.Entity<Driver>()
                .HasMany(x => x.WorkLocations)
                .WithMany(x => x.Drivers)
                .Map(
                    x =>
                    {
                        x.ToTable("dds_driverworklocations");
                        x.MapLeftKey("driver_Id");
                        x.MapRightKey("location_Id");
                    });

            #endregion DDS

            #region Sales and Marketing

            modelBuilder.Entity<SocialNetworkAccount>()
               .HasMany(x => x.Posts)
               .WithMany(x => x.SharingAccount)
               .Map(x =>
               {
                   x.ToTable("sm_SocialNetworkAccountSocialCampaignPosts");
                   x.MapLeftKey("SocialNetworkAccount_Id");
                   x.MapRightKey("SocialCampaignPost_Id");
               });
            modelBuilder.Entity<SalesMarketingWorkGroup>()
               .HasMany(x => x.Processes)
               .WithMany(x => x.WorkGroups)
               .Map(x =>
               {
                   x.ToTable("sm_WorkgroupProcess");
                   x.MapLeftKey("SalesMarketingWorkGroup_Id");
                   x.MapRightKey("SalesMarketingProcess_Id");
               });
            modelBuilder.Entity<SalesMarketingWorkGroup>()
               .HasMany(x => x.Members)
               .WithMany(x => x.SM_WorkGroupMembers)
               .Map(x =>
               {
                   x.ToTable("sm_WorkgroupsMembers");
                   x.MapLeftKey("SalesMarketingWorkGroup_Id");
                   x.MapRightKey("users_Id");
               });
            modelBuilder.Entity<SalesMarketingWorkGroup>()
               .HasMany(x => x.ReviewersApprovers)
               .WithMany(x => x.SM_WorkGroupApprovers)
               .Map(x =>
               {
                   x.ToTable("sm_WorkgroupsReviewersApprovers");
                   x.MapLeftKey("SalesMarketingWorkGroup_Id");
                   x.MapRightKey("users_Id");
               });
            modelBuilder.Entity<SocialCampaign>()
               .HasMany(x => x.TargetNetworks)
               .WithMany(x => x.SocialCampaigns)
               .Map(x =>
               {
                   x.ToTable("sm_TargetNetworkSocialCampaign");
                   x.MapLeftKey("SocialCampaign_Id");
                   x.MapRightKey("NetworkType_Id");
               });
            modelBuilder.Entity<SocialCampaign>()
               .HasMany(x => x.BrandProducts)
               .WithMany(x => x.SocialCampaigns)
               .Map(x =>
               {
                   x.ToTable("sm_BrandProductSocialCampaigns");
                   x.MapLeftKey("SocialCampaign_Id");
                   x.MapRightKey("BrandProduct_Id");
               });
            modelBuilder.Entity<SocialCampaign>()
               .HasMany(x => x.Attributes)
               .WithMany(x => x.SocialCampaigns)
               .Map(x =>
               {
                   x.ToTable("sm_AttributeSocialCampaigns");
                   x.MapLeftKey("SocialCampaign_Id");
                   x.MapRightKey("Attribute_Id");
               });
            modelBuilder.Entity<SocialCampaign>()
               .HasMany(x => x.ValuePropositons)
               .WithMany(x => x.SocialCampaigns)
               .Map(x =>
               {
                   x.ToTable("sm_ValuePropositionSocialCampaigns");
                   x.MapLeftKey("SocialCampaign_Id");
                   x.MapRightKey("ValueProposition_Id");
               });

            modelBuilder.Entity<ValueProposition>()
               .HasMany(x => x.Segments)
               .WithMany(x => x.ValuePropositions)
               .Map(x =>
               {
                   x.ToTable("sm_ValuePropositonSegments");
                   x.MapLeftKey("ValueProposition_Id");
                   x.MapRightKey("Segment_Id");
               });
            modelBuilder.Entity<SMContact>()
               .HasMany(x => x.Places)
               .WithMany(x => x.Contacts)
               .Map(x =>
               {
                   x.ToTable("sm_SMContactPlace");
                   x.MapLeftKey("SMContact_Id");
                   x.MapRightKey("Place_Id");
               });
            modelBuilder.Entity<SMContact>()
               .HasMany(x => x.Segments)
               .WithMany(x => x.Contacts)
               .Map(x =>
               {
                   x.ToTable("sm_SMContactSegments");
                   x.MapLeftKey("SMContact_Id");
                   x.MapRightKey("Segments_Id");
               });
            modelBuilder.Entity<Segment>()
               .HasMany(x => x.Areas)
               .WithMany(x => x.Segments)
               .Map(x =>
               {
                   x.ToTable("sm_SegmentsAreas");
                   x.MapLeftKey("Segments_Id");
                   x.MapRightKey("Areas_Id");
               });
            modelBuilder.Entity<Place>()
               .HasMany(x => x.Areas)
               .WithMany(x => x.Places)
               .Map(x =>
               {
                   x.ToTable("sm_PlacesAreas");
                   x.MapLeftKey("Places_Id");
                   x.MapRightKey("Areas_Id");
               });
            modelBuilder.Entity<Segment>()
               .HasMany(x => x.CampaignEmails)
               .WithMany(x => x.Segments)
               .Map(x =>
               {
                   x.ToTable("sm_SegmentsCampaignEmails");
                   x.MapLeftKey("Segments_Id");
                   x.MapRightKey("CampaignEmails_Id");
               });
            modelBuilder.Entity<Segment>()
               .HasMany(x => x.EmailCampaigns)
               .WithMany(x => x.Segments)
               .Map(x =>
               {
                   x.ToTable("sm_SegmentsEmailCampaigns");
                   x.MapLeftKey("Segments_Id");
                   x.MapRightKey("EmailCampaigns_Id");
               });

            #endregion Sales and Marketing

            #region Spannered

            modelBuilder.Entity<Asset>()
               .HasMany(x => x.Tags)
               .WithMany(x => x.Assets)
               .Map(x =>
               {
                   x.ToTable("sp_AssetTags");
                   x.MapLeftKey("Asset_Id");
                   x.MapRightKey("Tag_Id");
               });
            modelBuilder.Entity<Asset>()
              .HasMany(x => x.OtherAssets)
              .WithMany()
              .Map(x =>
              {
                  x.ToTable("sp_LinkAssets");
                  x.MapLeftKey("Asset_Id");
                  x.MapRightKey("Related_Id");
              });
            modelBuilder.Entity<SpanneredWorkgroup>()
               .HasMany(x => x.Processes)
               .WithMany(x => x.WorkGroups)
               .Map(x =>
               {
                   x.ToTable("sp_WorkgroupProcess");
                   x.MapLeftKey("Workgroup_Id");
                   x.MapRightKey("Process_Id");
               });
            modelBuilder.Entity<SpanneredWorkgroup>()
               .HasMany(x => x.Members)
               .WithMany(x => x.SPWorkGroupMembers)
               .Map(x =>
               {
                   x.ToTable("sp_WorkgroupsMembers");
                   x.MapLeftKey("Workgroup_Id");
                   x.MapRightKey("users_Id");
               });
            modelBuilder.Entity<SpanneredWorkgroup>()
               .HasMany(x => x.ReviewersApprovers)
               .WithMany(x => x.SPWorkGroupApprovers)
               .Map(x =>
               {
                   x.ToTable("sp_WorkgroupsApprovers");
                   x.MapLeftKey("Workgroup_Id");
                   x.MapRightKey("users_Id");
               });
            modelBuilder.Entity<SpanneredWorkgroup>()
              .HasMany(x => x.ProductGroups)
              .WithMany(x => x.SpanneredWorkgroups)
              .Map(x =>
              {
                  x.ToTable("sp_WorkgroupsProductGroups");
                  x.MapLeftKey("Workgroup_Id");
                  x.MapRightKey("TraderGroup_Id");
              });
            modelBuilder.Entity<Asset>()
              .HasMany(x => x.AssetTraderPurchases)
              .WithMany(x => x.Assets)
              .Map(x =>
              {
                  x.ToTable("sp_AssetsTraderPurchases");
                  x.MapLeftKey("Asset_Id");
                  x.MapRightKey("TraderPurchase_Id");
              });
            modelBuilder.Entity<Asset>()
              .HasMany(x => x.Transfers)
              .WithMany(x => x.Assets)
              .Map(x =>
              {
                  x.ToTable("sp_AssetsTransfers");
                  x.MapLeftKey("Asset_Id");
                  x.MapRightKey("Transfer_Id");
              });

            #endregion Spannered

            #region Operator

            modelBuilder.Entity<OperatorLocation>()
               .HasMany(x => x.Teams)
               .WithMany(x => x.Locations)
               .Map(x =>
               {
                   x.ToTable("op_LocationsTeamsRef");
                   x.MapLeftKey("Location_Id");
                   x.MapRightKey("TeamPerson_Id");
               });
            modelBuilder.Entity<OperatorRole>()
               .HasMany(x => x.Teams)
               .WithMany(x => x.Roles)
               .Map(x =>
               {
                   x.ToTable("op_RolesTeamsRef");
                   x.MapLeftKey("Role_Id");
                   x.MapRightKey("TeamPerson_Id");
               });
            modelBuilder.Entity<OperatorTag>()
               .HasMany(x => x.Goals)
               .WithMany(x => x.Tags)
               .Map(x =>
               {
                   x.ToTable("op_TagsGoalsRef");
                   x.MapLeftKey("Tag_Id");
                   x.MapRightKey("Goal_Id");
               });
            modelBuilder.Entity<OperatorTag>()
               .HasMany(x => x.FormDefinitions)
               .WithMany(x => x.Tags)
               .Map(x =>
               {
                   x.ToTable("op_TagsFormDefinitionsRef");
                   x.MapLeftKey("Tag_Id");
                   x.MapRightKey("FormDefinition_Id");
               });
            modelBuilder.Entity<ApprovalReq>()
              .HasMany(e => e.OperatorClockIn)
              .WithRequired(e => e.ApprovalTimeIn)
              .WillCascadeOnDelete(false);
            modelBuilder.Entity<ApprovalReq>()
              .HasMany(e => e.OperatorClockOut)
              .WithOptional(e => e.ApprovalTimeOut)
              .WillCascadeOnDelete(false);

            #endregion Operator

            #region B2B

            modelBuilder.Entity<QbicleDomain>()
              .HasMany(x => x.B2BQbicles)
              .WithMany(x => x.Domains)
              .Map(x =>
              {
                  x.ToTable("b2b_QbicleQbicleDomain");
                  x.MapLeftKey("QbicleDomain_Id");
                  x.MapRightKey("B2BQbicle_Id");
              });

            modelBuilder.Entity<PurchaseSalesPartnership>()
              .HasMany(x => x.Catalogs)
              .WithMany(x => x.PurchaseSalesPartnerships)
              .Map(x =>
              {
                  x.ToTable("b2b_PartnershipPosMenus");
                  x.MapLeftKey("Partnership_Id");
                  x.MapRightKey("PosMenu_Id");
              });
            modelBuilder.Entity<Partnership>()
                .HasRequired(x => x.ParentRelationship)
                .WithMany(x => x.Partnerships);

            modelBuilder.Entity<B2BCatalogItem>()
              .HasMany(x => x.ProviderLocations)
              .WithMany(x => x.B2BCatalogItems)
              .Map(x =>
              {
                  x.ToTable("b2b_CatalogItemLocations");
                  x.MapLeftKey("B2BCatalogItem_Id");
                  x.MapRightKey("Location_Id");
              });
            modelBuilder.Entity<B2BProfile>()
              .HasMany(x => x.DefaultB2BRelationshipManagers)
              .WithMany(x => x.B2bProfileManagers)
              .Map(x =>
              {
                  x.ToTable("b2b_DefaultB2BManagers");
                  x.MapLeftKey("B2BProfile_Id");
                  x.MapRightKey("User_Id");
              });
            modelBuilder.Entity<B2BProfile>()
              .HasMany(x => x.DefaultB2CRelationshipManagers)
              .WithMany(x => x.B2CProfileManagers)
              .Map(x =>
              {
                  x.ToTable("b2b_DefaultB2CManagers");
                  x.MapLeftKey("B2BProfile_Id");
                  x.MapRightKey("User_Id");
              });
            modelBuilder.Entity<B2BLogisticsAgreement>()
              .HasMany(x => x.ConsumerLocations)
              .WithMany(x => x.B2BLogisticsAgreements)
              .Map(x =>
              {
                  x.ToTable("b2b_LogisticsAgreementTraderLocations");
                  x.MapLeftKey("B2BLogisticsAgreement_Id");
                  x.MapRightKey("TraderLocation_Id");
              });

            modelBuilder.Entity<Partnership>()
              .HasMany(x => x.ProviderPartnershipManagers)
              .WithMany(x => x.B2BProviderPartnerships)
              .Map(x =>
              {
                  x.ToTable("b2b_ProviderPartnershipUsers");
                  x.MapLeftKey("B2BPartnership_Id");
                  x.MapRightKey("User_Id");
              });
            modelBuilder.Entity<Partnership>()
              .HasMany(x => x.ConsumerPartnershipManagers)
              .WithMany(x => x.B2BConsumerPartnerships)
              .Map(x =>
              {
                  x.ToTable("b2b_ConsumerPartnershipUsers");
                  x.MapLeftKey("B2BPartnership_Id");
                  x.MapRightKey("User_Id");
              });
            modelBuilder.Entity<B2BProfile>()
              .HasMany(x => x.BusinessLocations)
              .WithMany(x => x.B2BProfiles)
              .Map(x =>
              {
                  x.ToTable("b2b_ProfilesTraderLocations");
                  x.MapLeftKey("B2BProfile_Id");
                  x.MapRightKey("TraderLocation_Id");
              });
            modelBuilder.Entity<B2BProfile>()
              .HasMany(x => x.BusinessCatalogues)
              .WithMany(x => x.B2BProfiles)
              .Map(x =>
              {
                  x.ToTable("b2b_ProfilesCatalogs");
                  x.MapLeftKey("B2BProfile_Id");
                  x.MapRightKey("Catalog_Id");
              });

            #endregion B2B

            #region B2C And C2C

            modelBuilder.Entity<C2CQbicle>()
               .HasMany(x => x.Customers)
               .WithMany(x => x.C2CQbicles)
               .Map(x =>
               {
                   x.ToTable("c2c_c2cqbiclescustomers");
                   x.MapLeftKey("C2CQbicle_Id");
                   x.MapRightKey("User_Id");
               });
            modelBuilder.Entity<CQbicle>()
               .HasMany(x => x.LikedBy)
               .WithMany(x => x.CQbicles)
               .Map(x =>
               {
                   x.ToTable("c2c_cqbicleslikedby");
                   x.MapLeftKey("CQbicle_Id");
                   x.MapRightKey("User_Id");
               });

            #endregion B2C And C2C

            #region Catalog

            modelBuilder.Entity<VariantOption>()
               .HasMany(x => x.Variants)
               .WithMany(x => x.VariantOptions)
               .Map(x =>
               {
                   x.ToTable("VariantOptionPosVariants");
                   x.MapLeftKey("PosVariantOption_Id");
                   x.MapRightKey("PosVariant_Id");
               });
            modelBuilder.Entity<Catalog>()
                .HasMany(x => x.OrderItemDimensions)
                .WithMany(x => x.PosMenus)
                .Map(x =>
                {
                    x.ToTable("CatalogTransactionDimensions");
                    x.MapLeftKey("PosMenu_Id");
                    x.MapRightKey("TransactionDimension_Id");
                });

            #endregion Catalog

            #region Loyalty

            modelBuilder.Entity<BulkDealVoucherInfo>()
              .HasMany(x => x.Businesses)
              .WithMany(x => x.BulkDealVoucherInfos)
              .Map(x =>
              {
                  x.ToTable("loy_bulkdeal_voucherInfo_business");
                  x.MapLeftKey("BulkDealVoucherInfo_Id");
                  x.MapRightKey("Business_Id");
              });

            modelBuilder.Entity<LoyaltyPromotion>()
              .HasMany(x => x.LikingUsers)
              .WithMany(x => x.LikedPromotions)
              .Map(x =>
              {
                  x.ToTable("loy_PromotionsUsersXref");
                  x.MapLeftKey("Promotion_Id");
                  x.MapRightKey("User_Id");
              });
            modelBuilder.Entity<LoyaltyPromotion>()
              .HasMany(x => x.LikedBy)
              .WithMany(x => x.MarkedLikedPromotions)
              .Map(x =>
              {
                  x.ToTable("loy_MarkedLikedPromotionsUsersXref");
                  x.MapLeftKey("Promotion_Id");
                  x.MapRightKey("User_Id");
              });
            modelBuilder.Entity<VoucherInfo>()
              .HasMany(x => x.Locations)
              .WithMany(x => x.VoucherInfos)
              .Map(x =>
              {
                  x.ToTable("loy_VouchersLocationsXref");
                  x.MapLeftKey("Voucher_Id");
                  x.MapRightKey("Location_Id");
              });

            #endregion Loyalty

            #region HighlightPost

            modelBuilder.Entity<RealEstateListingHighlight>()
              .HasMany(x => x.IncludedProperties)
              .WithMany(x => x.RealEstates)
              .Map(x =>
              {
                  x.ToTable("hl_RealEstatePropertyExtras");
                  x.MapLeftKey("RealEstate_Id");
                  x.MapRightKey("PropertyExtra_Id");
              });

            #endregion HighlightPost
        }

        #region Community

        public DbSet<CommunityPage> CommunityPages { get; set; }
        public DbSet<DomainProfile> DomainProfiles { get; set; }
        public DbSet<UserProfilePage> UserProfilePages { get; set; }
        public DbSet<Employment> Employments { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<KeyWord> KeyWords { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<ProfileFile> ProfileFiles { get; set; }

        //public DbSet<Skill> Skills { get; set; }
        public DbSet<Tag> Tags { get; set; }

        public DbSet<TalentPool> TalentPools { get; set; }

        #endregion Community

        #region Qbicles

        public DbSet<MediaFolder> MediaFolders { get; set; }
        public DbSet<ApplicationUser> QbicleUser { get; set; }
        public DbSet<QbicleDomain> Domains { get; set; }
        public DbSet<QbicleDomainRequest> QbicleDomainRequests { get; set; }
        public DbSet<Customer> PaystackCustomers { get; set; }
        public DbSet<DomainExtensionRequest> DomainExtensionRequests { get; set; }
        public DbSet<Qbicle> Qbicles { get; set; }
        public DbSet<QbicleTask> QbicleTasks { get; set; }
        public DbSet<QbicleAlert> Alerts { get; set; }
        public DbSet<QbicleEvent> Events { get; set; }
        public DbSet<QbicleMedia> Medias { get; set; }
        public DbSet<QbiclePost> Posts { get; set; }
        public DbSet<QbicleActivity> Activities { get; set; }
        public DbSet<QbicleDiscussion> Discussions { get; set; }
        public DbSet<EmailConfiguration> EmailConfigurations { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<QbicleLog> QbicleLogs { get; set; }
        public DbSet<DomainAccessLog> DomainAccessLogs { get; set; }
        public DbSet<QbicleAccessLog> QbicleAccessLogs { get; set; }
        public DbSet<AppAccessLog> AppAccessLogs { get; set; }
        public DbSet<QbicleFileType> QbicleFileTypes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<MyDesk> MyDesks { get; set; }
        public DbSet<MyTag> MyFolders { get; set; }
        public DbSet<MyPin> MyPins { get; set; }
        public DbSet<SubscriptionAccount> SubscriptionAccounts { get; set; }
        public DbSet<AccountPackage> AccountPackages { get; set; }
        public DbSet<TaskFormDefinitionRef> TaskFormDefinitionRef { get; set; }
        public DbSet<ApprovalReqFormRef> ApprovalReqFormRef { get; set; }

        public DbSet<FormManager> FormManager { get; set; }
        public DbSet<QbicleApplication> Applications { get; set; }
        public DbSet<SubscribedAppsLog> SubscribedAppsLogs { get; set; }
        public DbSet<AppRight> AppRight { get; set; }
        public DbSet<RoleRightAppXref> RoleRightAppXref { get; set; }
        public DbSet<DomainRole> DomainRole { get; set; }
        public DbSet<AppInstance> AppInstances { get; set; }

        public DbSet<ApprovalReq> ApprovalReqs { get; set; }
        public DbSet<ApprovalReqHistory> ApprovalReqHistories { get; set; }

        public DbSet<Approval> ApprovalApp { get; set; }
        public DbSet<ApprovalGroup> ApprovalAppsGroup { get; set; }
        public DbSet<ApprovalRequestDefinition> ApprovalAppsRequestDefinition { get; set; }
        public DbSet<ApprovalDocument> ApprovalDocument { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<VersionedFile> VersionedFiles { get; set; }

        public DbSet<StorageFile> StorageFiles { get; set; }
        public DbSet<StorageFileDetail> StorageFileDetails { get; set; }

        //Qbicles root V2
        public DbSet<QbicleJob> Jobs { get; set; }

        public DbSet<QbicleSet> Sets { get; set; }
        public DbSet<QbiclePeople> People { get; set; }
        public DbSet<QbiclePerformance> Performances { get; set; }
        public DbSet<QbicleRecurrance> Recurrances { get; set; }
        public DbSet<QbicleRelated> Relateds { get; set; }
        public DbSet<QbicleStep> Steps { get; set; }
        public DbSet<QbicleStepInstance> Stepinstances { get; set; }
        public DbSet<QbicleTimeSpent> Timespents { get; set; }
        public DbSet<QbicleLink> QbicleLinks { get; set; }
        public DbSet<Qbicles.Models.Invitation.Invitation> Invitations { get; set; }
        public DbSet<InvitationSentLog> InvitationSentLogs { get; set; }
        public DbSet<UiSetting> UiSettings { get; set; }
        public virtual DbSet<ViewDatesQbicleStream> ViewLstDatesQbicles { get; set; }
        public virtual DbSet<ViewQbicleActivitiesStream> ViewQbicleActivitiesActivities { get; set; }
        public virtual DbSet<UnusedInventoryView> UnusedInventoriesView { get; set; }
        public virtual DbSet<UnusedBatchesView> UnusedBatchesView { get; set; }
        public DbSet<UserAvatars> UserAvatars { get; set; }
        public DbSet<CurrencySetting> CurrencySettings { get; set; }
        public DbSet<UserConnectedSignalR> ConnectedSignalRs { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<SystemDomain> SystemDomains { get; set; }
        public DbSet<InviteCount> InviteCounts { get; set; }
        public DbSet<SystemToken> SystemTokens { get; set; }
        public DbSet<BusinessCategory> BusinessCategories { get; set; }
        public DbSet<PinVerification> PinVerifications { get; set; }
        public DbSet<Subscription> DomainSubscriptions { get; set; }
        public DbSet<DomainPlan> DomainPlans { get; set; }
        public DbSet<BusinessDomainLevel> BusinessDomainLevels { get; set; }
        //end

        #endregion Qbicles

        #region Cleanbooks

        public virtual DbSet<CBWorkGroup> CBWorkGroups { get; set; }
        public virtual DbSet<CBProcess> CBProcesses { get; set; }
        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<accountgroup> accountgroups { get; set; }
        public virtual DbSet<accountupdatefrequency> accountupdatefrequencies { get; set; }
        public virtual DbSet<dateformat> dateformats { get; set; }
        public virtual DbSet<deletedaccount> deletedaccounts { get; set; }
        public virtual DbSet<deletedtask> deletedtasks { get; set; }
        public virtual DbSet<deletedupload> deleteduploads { get; set; }
        public virtual DbSet<filetype> filetypes { get; set; }
        public virtual DbSet<profile> profiles { get; set; }
        public virtual DbSet<project> projects { get; set; }
        public virtual DbSet<projectgroup> projectgroups { get; set; }
        public virtual DbSet<projectnotificationinterval> projectnotificationintervals { get; set; }
        public virtual DbSet<projecttaskxref> projecttaskxrefs { get; set; }
        public virtual DbSet<reasonsentemail> reasonsentemails { get; set; }
        public virtual DbSet<scheduledemail> scheduledemails { get; set; }
        public virtual DbSet<scheduledaccountemail> scheduledaccountemails { get; set; }
        public virtual DbSet<task> tasks { get; set; }
        public virtual DbSet<taskaccount> taskaccounts { get; set; }
        public virtual DbSet<taskexecutioninterval> taskexecutionintervals { get; set; }
        public virtual DbSet<taskgroup> taskgroups { get; set; }
        public virtual DbSet<taskinstance> taskinstances { get; set; }
        public virtual DbSet<taskinstancedaterange> taskinstancedateranges { get; set; }
        public virtual DbSet<tasktype> tasktypes { get; set; }
        public virtual DbSet<transaction> transactions { get; set; }

        public virtual DbSet<transactionanalysisclassificationbydate> transactionanalysisclassificationbydates
        {
            get;
            set;
        }

        public virtual DbSet<transactionanalysisclassificationbyrange> transactionanalysisclassificationbyranges
        {
            get;
            set;
        }

        public virtual DbSet<transactionanalysisclassificationbytype> transactionanalysisclassificationbytypes
        {
            get;
            set;
        }

        public virtual DbSet<transactionanalysiscomment> transactionanalysiscomments { get; set; }
        public virtual DbSet<transactionanalysisrecord> transactionanalysisrecords { get; set; }
        public virtual DbSet<transactionanalysisreportgroup> transactionanalysisreportgroups { get; set; }
        public virtual DbSet<transactionanalysisreportstatistic> transactionanalysisreportstatistics { get; set; }
        public virtual DbSet<transactionanalysistask> transactionanalysistasks { get; set; }
        public virtual DbSet<transactionanalysistaskprofilexref> transactionanalysistaskprofilexrefs { get; set; }
        public virtual DbSet<transactionmatchingcopiedrecord> transactionmatchingcopiedrecords { get; set; }
        public virtual DbSet<transactionmatchinggroup> transactionmatchinggroups { get; set; }
        public virtual DbSet<transactionmatchingmatched> transactionmatchingmatcheds { get; set; }
        public virtual DbSet<transactionmatchingmethod> transactionmatchingmethods { get; set; }
        public virtual DbSet<transactionmatchingrecord> transactionmatchingrecords { get; set; }
        public virtual DbSet<transactionmatchingrelationship> transactionmatchingrelationships { get; set; }
        public virtual DbSet<transactionmatchingtask> transactionmatchingtasks { get; set; }
        public virtual DbSet<transactionmatchingtype> transactionmatchingtypes { get; set; }
        public virtual DbSet<transactionmatchingunmatched> transactionmatchingunmatcheds { get; set; }
        public virtual DbSet<upload> uploads { get; set; }
        public virtual DbSet<uploadfield> uploadfields { get; set; }
        public virtual DbSet<UploadFormat> uploadformats { get; set; }
        public virtual DbSet<fcaccounttrendvalues> fcaccounttrendvaluess { get; set; }
        public virtual DbSet<fcprofiletrendvalues> fcprofiletrendvaluess { get; set; }
        public virtual DbSet<fcratioaccount> fcratioaccounts { get; set; }
        public virtual DbSet<fcratioaccountvalue> fcratioaccountvalues { get; set; }
        public virtual DbSet<fcratiomanualbalance> fcratiomanualbalances { get; set; }
        public virtual DbSet<fcratiomanualbalancevalue> fcratiomanualbalancevalues { get; set; }
        public virtual DbSet<fcratioprofile> fcratioprofiles { get; set; }
        public virtual DbSet<fcratioprofiletaskxref> fcratioprofiletaskxrefs { get; set; }
        public virtual DbSet<fcratioprofilevalue> fcratioprofilevalues { get; set; }
        public virtual DbSet<fcreportexecutioninstance> fcreportexecutioninstances { get; set; }
        public virtual DbSet<fcreporttransactionxref> fcreporttransactionxrefs { get; set; }
        public virtual DbSet<fctotalaccountbalance> fctotalaccountbalances { get; set; }
        public virtual DbSet<fctotalmanualbalancevalue> fctotalmanualbalancevalues { get; set; }
        public virtual DbSet<fctotalprofiletaskxref> fctotalprofiletaskxrefs { get; set; }
        public virtual DbSet<fctotalprofiletotal> fctotalprofiletotals { get; set; }
        public virtual DbSet<finacialcontroltask> finacialcontroltasks { get; set; }
        public virtual DbSet<financialcontrolaccounttrend> financialcontrolaccounttrends { get; set; }
        public virtual DbSet<financialcontrolbalanceaccount> financialcontrolbalanceaccounts { get; set; }
        public virtual DbSet<financialcontrolmanualbalance> financialcontrolmanualbalances { get; set; }
        public virtual DbSet<financialcontrolprofiletrend> financialcontrolprofiletrends { get; set; }
        public virtual DbSet<financialcontrolratio> financialcontrolratios { get; set; }

        public virtual DbSet<financialcontrolrecenttransactionaccounts> financialcontrolrecenttransactionaccountss
        {
            get;
            set;
        }

        public virtual DbSet<financialcontrolreportdefinition> financialcontrolreportdefinitions { get; set; }
        public virtual DbSet<financialcontroltotalprofile> financialcontroltotalprofiles { get; set; }
        public virtual DbSet<financialcontroltrendprofiletaskxref> financialcontroltrendprofiletaskxrefs { get; set; }
        public virtual DbSet<manualbalance> manualbalances { get; set; }
        public virtual DbSet<manualbalancegroup> manualbalancegroups { get; set; }
        public virtual DbSet<audit_user> audit_users { get; set; }
        public virtual DbSet<audit_account> audit_accounts { get; set; }
        public virtual DbSet<audit_task> audit_tasks { get; set; }
        public virtual DbSet<audit_transaction_analysis> audit_transaction_analysiss { get; set; }
        public virtual DbSet<audit_transaction_matching> audit_transaction_matchings { get; set; }
        public virtual DbSet<transactionmatchingtaskrulesacces> transactionmatchingtaskrulesaccess { get; set; }
        public virtual DbSet<transactionmatchingrule> transactionmatchingrules { get; set; }

        public virtual DbSet<transactionmatchingamountvariancevalue> transactionmatchingamountvariancevalues
        {
            get;
            set;
        }

        public virtual DbSet<transactionmatchingdatevariancevalue> transactionmatchingdatevariancevalues { get; set; }
        public virtual DbSet<transactionmatchingtaskrule> transactionmatchingtaskrules { get; set; }
        public virtual DbSet<tmtaskalerteexref> tmtaskalerteexrefs { get; set; }
        public virtual DbSet<fcautoexecute> fcautoexecutes { get; set; }
        public virtual DbSet<singleaccountalerttransanalysisxref> singleaccountalerttransanalysisxrefs { get; set; }
        public virtual DbSet<singleaccountalertusersxref> singleaccountalertusersxrefs { get; set; }
        public virtual DbSet<singleaccountalert> singleaccountalerts { get; set; }
        public virtual DbSet<alertcondition> alertconditions { get; set; }
        public virtual DbSet<multipleaccountalertuserxref> multipleaccountalertuserxrefs { get; set; }
        public virtual DbSet<multipleaccountalert> multipleaccountalerts { get; set; }
        public virtual DbSet<alertmultipleaccount> alertmultipleaccounts { get; set; }
        public virtual DbSet<alertmultipleprofile> alertmultipleprofiles { get; set; }
        public virtual DbSet<alertmultipleprofiletaskxref> alertmultipleprofiletaskxrefs { get; set; }
        public virtual DbSet<balanceanalysisaction> balanceanalysisactions { get; set; }
        public virtual DbSet<balanceanalysiscomment> balanceanalysiscomments { get; set; }
        public virtual DbSet<balanceanalysisdataset> balanceanalysisdatasets { get; set; }
        public virtual DbSet<balanceanalysisdocument> balanceanalysisdocuments { get; set; }
        public virtual DbSet<balanceanalysismappingrule> balanceanalysismappingrules { get; set; }
        public virtual DbSet<balanceanalysismatch> balanceanalysismatchs { get; set; }
        public virtual DbSet<balanceanalysispredefaction> balanceanalysispredefactions { get; set; }
        public virtual DbSet<balanceanalysistask> balanceanalysistasks { get; set; }
        public virtual DbSet<balanceanalysiswarninglevel> balanceanalysiswarninglevels { get; set; }
        public virtual DbSet<transactionmatchingunmatchgroup> transactionmatchingunmatchgroups { get; set; }

        public virtual DbSet<BookClosure> BookClosures { get; set; }

        #endregion Cleanbooks

        #region Bookkeeping

        public virtual DbSet<BKAccount> BKAccounts { get; set; }
        public virtual DbSet<BKAppSettings> BKAppSettings { get; set; }
        public virtual DbSet<BKGroup> BKGroups { get; set; }
        public virtual DbSet<BKSubGroup> BKSubGroups { get; set; }
        public virtual DbSet<BKTransaction> BKTransactions { get; set; }
        public virtual DbSet<CoANode> BKCoANodes { get; set; }
        public virtual DbSet<JournalEntry> JournalEntrys { get; set; }
        public virtual DbSet<JournalEntryTemplate> JournalEntryTemplates { get; set; }
        public virtual DbSet<JournalEntryTemplateRow> JournalEntryTemplateRows { get; set; }
        public virtual DbSet<JournalGroup> JournalGroups { get; set; }
        public virtual DbSet<TaxRate> TaxRates { get; set; }
        public virtual DbSet<TransactionDimension> TransactionDimensions { get; set; }
        public virtual DbSet<BookkeepingProcess> BookkeepingProcesses { get; set; }
        public virtual DbSet<BKWorkGroup> BKWorkGroups { get; set; }
        public virtual DbSet<IncomeStatementReportEntry> BKIncomeReportEntries { get; set; }
        public virtual DbSet<IncomeStatementReportTemplate> BKIncomeReportTemplates { get; set; }
        public virtual DbSet<InlineReportEntry> BKInlineReportEntries { get; set; }
        public virtual DbSet<TraderJournalEntryLog> TraderJournalEntryLogs { get; set; }
        public virtual DbSet<SaleTransferBookkeepingLog> SaleTransferBookkeepingLogs { get; set; }
        public virtual DbSet<SaleInvoiceBookkeepingLog> SaleInvoiceBookkeepingLogs { get; set; }
        public virtual DbSet<PurchaseNonInvBookkeepingLog> PurchaseNoninvBookkeepingLogs { get; set; }
        public virtual DbSet<PurchaseInventoryBookkeepingLog> PurchaseInventoryBookkeepingLogs { get; set; }
        public virtual DbSet<PaymentBookkeepingLog> PaymentBookkeepingLogs { get; set; }
        public virtual DbSet<ManufacturingBookkeepingLog> ManufacturingBookkeepingLogs { get; set; }

        #endregion Bookkeeping

        #region Trader

        public virtual DbSet<TraderReference> TraderReferences { get; set; }
        public virtual DbSet<TraderProcess> TraderProcesses { get; set; }
        public virtual DbSet<ProductUnit> ProductUnits { get; set; }
        public virtual DbSet<Ingredient> Ingredients { get; set; }
        public virtual DbSet<InventoryDetail> InventoryDetails { get; set; }
        public virtual DbSet<Recipe> Recipes { get; set; }
        public virtual DbSet<TraderContact> TraderContacts { get; set; }
        public virtual DbSet<TraderContactRef> TraderContactRefs { get; set; }
        public virtual DbSet<TraderItemVendor> TraderItemVendors { get; set; }
        public virtual DbSet<TraderContactGroup> TraderContactGroups { get; set; }
        public virtual DbSet<TraderGroup> TraderGroups { get; set; }
        public virtual DbSet<TraderItem> TraderItems { get; set; }
        public virtual DbSet<ProductGalleryItem> ProductGalleryItems { get; set; }
        public virtual DbSet<TraderItemImport> TraderItemImports { get; set; }
        public virtual DbSet<TraderLocation> TraderLocations { get; set; }
        public virtual DbSet<TraderSale> TraderSales { get; set; }
        public virtual DbSet<TraderTransactionItem> TraderSaleItems { get; set; }
        public virtual DbSet<TraderPurchase> TraderPurchases { get; set; }
        public virtual DbSet<TraderSettings> TraderSettings { get; set; }
        public virtual DbSet<TraderAddress> TraderAddress { get; set; }
        public virtual DbSet<WorkGroup> WorkGroups { get; set; }
        public virtual DbSet<TraderTransfer> TraderTransfers { get; set; }
        public virtual DbSet<TraderTransferItem> TraderTransferItems { get; set; }
        public virtual DbSet<TraderCashAccount> TraderCashAccounts { get; set; }
        public virtual DbSet<CashAccountTransaction> CashAccountTransactions { get; set; }
        public virtual DbSet<Invoice> Invoices { get; set; }
        public virtual DbSet<InvoiceLog> InvoiceLogs { get; set; }
        public virtual DbSet<InvoiceProcessLog> InvoiceProcessLogs { get; set; }
        public virtual DbSet<InvoiceTransactionItems> InvoiceTransactionItems { get; set; }
        public virtual DbSet<TraderSalesOrder> TraderSalesOrders { get; set; }
        public virtual DbSet<TraderPurchaseOrder> TraderPurchaseOrders { get; set; }
        public virtual DbSet<Batch> InventoryBatches { get; set; }
        public virtual DbSet<BatchLog> InventoryBatchLogs { get; set; }
        public virtual DbSet<InventoryUpdateLog> InventoryUpdateLogs { get; set; }
        public virtual DbSet<InventoryDetailLog> InventoryDetailLogs { get; set; }
        public virtual DbSet<Shipment> Shipments { get; set; }
        public virtual DbSet<TransferLog> TraderTransferLogs { get; set; }
        public virtual DbSet<TransferProcessLog> TraderTransferProcessLogs { get; set; }
        public virtual DbSet<Price> TraderPrices { get; set; }
        public virtual DbSet<PriceTax> TraderPriceTaxes { get; set; }
        public virtual DbSet<PriceBook> TraderPriceBooks { get; set; }
        public virtual DbSet<PriceBookInstance> TraderPriceBookInstances { get; set; }
        public virtual DbSet<PriceBookVersion> PriceBookVersions { get; set; }
        public virtual DbSet<PriceBookPrice> TraderPriceBookPrices { get; set; }
        public virtual DbSet<PriceLog> TraderPriceLogs { get; set; }
        public virtual DbSet<ProductGroupPriceDefaults> TraderProductGroupPriceDefaults { get; set; }
        public virtual DbSet<ContactApprovalDefinition> ContactApprovalDefinitions { get; set; }
        public virtual DbSet<InvoiceApprovalDefinition> InvoiceApprovalDefinitions { get; set; }
        public virtual DbSet<PaymentApprovalDefinition> PaymentApprovalDefinitions { get; set; }
        public virtual DbSet<PurchaseApprovalDefinition> PurchaseApprovalDefinitions { get; set; }
        public virtual DbSet<SalesApprovalDefinition> SalesApprovalDefinitions { get; set; }
        public virtual DbSet<SalesReturnApprovalDefinition> SalesReturnApprovalDefinitions { get; set; }
        public virtual DbSet<SpotCountApprovalDefinition> SpotCountApprovalDefinitions { get; set; }
        public virtual DbSet<WasteReportApprovalDefinition> WasteReportApprovalDefinitions { get; set; }
        public virtual DbSet<TransferApprovalDefinition> TransferApprovalDefinitions { get; set; }
        public virtual DbSet<SpotCount> SpotCounts { get; set; }

        public virtual DbSet<SpotCountLog> SpotCountLogs { get; set; }
        public virtual DbSet<SpotCountProcessLog> SpotCountProcessLogs { get; set; }
        public virtual DbSet<StockAudit> StockAudits { get; set; }
        public virtual DbSet<StockAuditItem> StockAuditItems { get; set; }
        public virtual DbSet<SpotCountItem> SpotCountItems { get; set; }
        public virtual DbSet<WasteItem> WasteItems { get; set; }
        public virtual DbSet<WasteReport> WasteReports { get; set; }
        public virtual DbSet<ManufacturingLog> ManufacturingLogs { get; set; }
        public virtual DbSet<ManuJob> ManuJobs { get; set; }
        public virtual DbSet<ManuProcessLog> ManuProcessLogs { get; set; }
        public virtual DbSet<ManuApprovalDefinition> ManuApprovalDefinitions { get; set; }

        public virtual DbSet<PurchaseLog> TraderPurchaseLogs { get; set; }
        public virtual DbSet<PurchaseProcessLog> TraderPurchaseProcessLogs { get; set; }
        public virtual DbSet<SaleLog> TraderSaleLogs { get; set; }
        public virtual DbSet<SaleProcessLog> TraderSaleProcessLogs { get; set; }
        public virtual DbSet<TransactionItemLog> TraderTransactionItemLogs { get; set; }
        public virtual DbSet<WasteReportLog> WasteReportLogs { get; set; }
        public virtual DbSet<WasteReportProcessLog> WasteReportProcessLogs { get; set; }
        public virtual DbSet<WasteItemLog> WasteItemLogs { get; set; }
        public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }
        public virtual DbSet<PosPaymentMethodAccountXref> PosPaymentMethodAccountXrefs { get; set; }
        public virtual DbSet<PaymentProcessLog> PaymentProcessLogs { get; set; }
        public virtual DbSet<CashAccountTransactionLog> CashAccountTransactionLogs { get; set; }

        public virtual DbSet<BalanceAllocation> BalanceAllocations { get; set; }
        public virtual DbSet<CreditNote> CreditNotes { get; set; }
        public virtual DbSet<CreditNoteApprovalDefinition> CreditNoteApprovalDefinitions { get; set; }
        public virtual DbSet<ShiftAuditApprovalDefinition> ShiftAuditApprovalDefinitions { get; set; }

        public virtual DbSet<BudgetScenarioItemsApprovalDefinition> BudgetScenarioItemsApprovalDefinitions { get; set; }
        public virtual DbSet<BudgetScenarioItemGroupLog> BudgetScenarioItemGroupLogs { get; set; }
        public virtual DbSet<BudgetScenarioItemGroupProcessLog> BudgetScenarioItemGroupProcessLogs { get; set; }
        public virtual DbSet<BudgetScenarioItemLog> BudgetScenarioItemLogs { get; set; }
        public virtual DbSet<GroupSetting> MasterGroupSettings { get; set; }
        public virtual DbSet<MasterSetup> MasterSetups { get; set; }

        public virtual DbSet<Till> Tills { get; set; }
        public virtual DbSet<Safe> Safes { get; set; }
        public virtual DbSet<Checkpoint> Checkpoints { get; set; }
        public virtual DbSet<TillPayment> TillPayments { get; set; }
        public virtual DbSet<TillPaymentLog> TillPaymentLogs { get; set; }
        public virtual DbSet<TillPaymentProcessLog> TillPaymentProcessLogs { get; set; }
        public virtual DbSet<TillPaymentApprovalDefinition> TillPaymentApprovalDefinitions { get; set; }

        public virtual DbSet<Reorder> Reorders { get; set; }
        public virtual DbSet<ReorderItem> ReorderItems { get; set; }
        public virtual DbSet<ReorderItemGroup> ReorderItemGroups { get; set; }

        public virtual DbSet<FeaturedProduct> FeaturedProducts { get; set; }
        public virtual DbSet<FeaturedStore> FeaturedStores { get; set; }

        /// <summary>
        /// Featured Product Image
        /// </summary>
        public virtual DbSet<FeaturedProductImage> FPImages { get; set; }

        /// <summary>
        /// Featured Product -> Product
        /// </summary>
        public virtual DbSet<Product> FPProducts { get; set; }

        #endregion Trader

        #region Trader Resource

        public virtual DbSet<ResourceImage> ResourceImages { get; set; }
        public virtual DbSet<AccessArea> AccessAreas { get; set; }
        public virtual DbSet<ResourceCategory> ResourceCategorys { get; set; }
        public virtual DbSet<ResourceDocument> ResourceDocuments { get; set; }
        public virtual DbSet<AdditionalInfo> AdditionalInfos { get; set; }

        #endregion Trader Resource

        #region Trader Budgets

        public virtual DbSet<BudgetGroup> BudgetGroups { get; set; }
        public virtual DbSet<BudgetScenario> BudgetScenarios { get; set; }
        public virtual DbSet<BudgetScenarioItem> BudgetScenarioItems { get; set; }
        public virtual DbSet<BudgetScenarioItemGroup> BudgetScenarioItemGroups { get; set; }
        public virtual DbSet<ItemProjection> ItemProjections { get; set; }
        public virtual DbSet<PaymentTerms> PaymentTerms { get; set; }
        public virtual DbSet<ReportingPeriod> ReportingPeriods { get; set; }

        public virtual DbSet<ScenarioItemStartingQuantity> ScenarioItemStartingQuantities { get; set; }

        #endregion Trader Budgets

        #region Trader ODS

        public virtual DbSet<PrepQueue> PrepQueues { get; set; }
        public virtual DbSet<PrepQueueArchive> PrepQueueArchives { get; set; }
        public virtual DbSet<PrepDisplayDevice> PrepDisplayDevices { get; set; }
        public virtual DbSet<QueueOrder> QueueOrders { get; set; }
        public virtual DbSet<QueueOrderItem> QueueOrderItems { get; set; }
        public virtual DbSet<QueueExtra> QueueExtras { get; set; }

        public virtual DbSet<OrderTax> OrderTaxs { get; set; }

        public virtual DbSet<OrderCustomer> OrderCustomers { get; set; }

        public virtual DbSet<OrderPayment> OrderPayments { get; set; }
        public virtual DbSet<OdsDeviceType> OdsDeviceTypes { get; set; }

        public virtual DbSet<SplitAmount> SplitAmounts { get; set; }

        #endregion Trader ODS

        #region Trader DDS

        public virtual DbSet<DdsDevice> DdsDevice { get; set; }
        public virtual DbSet<Delivery> Deliveries { get; set; }
        public virtual DbSet<DeliveryQueue> DeliveryQueues { get; set; }
        public virtual DbSet<DeliveryQueueArchive> DeliveryQueueArchives { get; set; }
        public virtual DbSet<Driver> Drivers { get; set; }
        public virtual DbSet<DriverLog> DriverLogs { get; set; }
        public virtual DbSet<DriverGroup> DriverGroups { get; set; }
        public virtual DbSet<DeliverySettings> DeliverySettings { get; set; }

        #endregion Trader DDS

        #region Trader My Desk

        //public virtual DbSet<OnlineStore> OnlineStores { get; set; }
        //Trader Sale channel Config

        //public virtual DbSet<MyDeskOrder> MyDeskOrders { get; set; }

        #endregion Trader My Desk

        #region Form

        public virtual DbSet<DomainForm> DomainForm { get; set; }
        public virtual DbSet<FormDefinition> FormDefinition { get; set; }
        public virtual DbSet<FormElement> FormElement { get; set; }
        public virtual DbSet<FormElementData> FormElementData { get; set; }
        public virtual DbSet<FormInstance> FormInstances { get; set; }
        public virtual DbSet<Image> FormImages { get; set; }
        public virtual DbSet<Document> FormDocuments { get; set; }
        public virtual DbSet<Note> FormNotes { get; set; }

        #endregion Form

        #region PoS

        public virtual DbSet<PosOrderType> PosOrderTypes { get; set; }
        public virtual DbSet<PosDeviceType> PosDeviceTypes { get; set; }
        public virtual DbSet<PosDevice> PosDevices { get; set; }
        public virtual DbSet<PosDeviceOrderXref> PosDeviceOrderXrefs { get; set; }
        public virtual DbSet<PosApiRequestLog> PosApiRequestLogs { get; set; }
        public virtual DbSet<PosUserSession> PosUserSessions { get; set; }

        public virtual DbSet<PosAdministrator> PosAdministrators { get; set; }

        public virtual DbSet<PosTillManager> PosTillManagers { get; set; }
        public virtual DbSet<PosSupervisor> PosSupervisors { get; set; }
        public virtual DbSet<PosCashier> PosCashiers { get; set; }

        /// <summary>
        /// POS User
        /// </summary>
        public virtual DbSet<DeviceUser> DeviceUsers { get; set; }

        public virtual DbSet<PosOrderReference> PosOrderReferences { get; set; }

        public virtual DbSet<POSTable> PosTables { get; set; }
        public virtual DbSet<POSTableLayout> PosTableLayouts { get; set; }
        public virtual DbSet<PosOrderCancel> PosOrderCancels { get; set; }
        public virtual DbSet<DeliverySystemSetting> DeliverySystemSettings { get; set; }
        public virtual DbSet<PosOrderPrintCheck> PosOrderPrintChecks { get; set; }

        #endregion PoS

        #region Catalog

        public virtual DbSet<Category> PosCategories { get; set; }
        public virtual DbSet<CategoryItem> PosCategoryItems { get; set; }
        public virtual DbSet<Extra> PosExtras { get; set; }
        public virtual DbSet<Catalog> PosMenus { get; set; }
        public virtual DbSet<Variant> PosVariants { get; set; }
        public virtual DbSet<VariantOption> PosVariantOptions { get; set; }
        public virtual DbSet<VariantProperty> PosVariantProperties { get; set; }
        public virtual DbSet<CatalogPrice> CatalogPrices { get; set; }

        #endregion Catalog

        #region Sales and Marketing

        public virtual DbSet<NetworkType> NetworkTypes { get; set; }

        public virtual DbSet<SalesMarketingProcess> SalesMarketingProcesses { get; set; }

        public virtual DbSet<Settings> SalesMarketingSettings { get; set; }

        public virtual DbSet<SocialNetworkAccount> SocialNetworkAccounts { get; set; }

        public virtual DbSet<SocialCampaign> SocialCampaigns { get; set; }

        public virtual DbSet<SalesMarketingWorkGroup> SalesMarketingWorkGroups { get; set; }

        public virtual DbSet<SocialCampaignPost> SocialCampaignPosts { get; set; }

        public virtual DbSet<SocialCampaignQueue> SocialCampaignQueues { get; set; }

        public virtual DbSet<CampaignPostApproval> SocialCampaignApprovals { get; set; }

        public virtual DbSet<EmailCampaign> EmailCampaigns { get; set; }

        public virtual DbSet<EmailCampaignQueue> EmailCampaignQueues { get; set; }

        public virtual DbSet<CampaignEmail> CampaignEmails { get; set; }

        public virtual DbSet<EmailPostApproval> EmailPostApproval { get; set; }

        public virtual DbSet<FaceBookAccount> FaceBookAccounts { get; set; }
        public virtual DbSet<TwitterAccount> TwitterAccounts { get; set; }
        public virtual DbSet<SocialNetworkSystemSettings> SocialNetworkSystemSettings { get; set; }
        public virtual DbSet<Brand> SMBrands { get; set; }
        public virtual DbSet<BrandProduct> SmBrandProducts { get; set; }
        public virtual DbSet<Attribute> SMAttributes { get; set; }
        public virtual DbSet<AttributeGroup> SMAttributeGroups { get; set; }
        public virtual DbSet<IdeaTheme> SMIdeaThemes { get; set; }
        public virtual DbSet<IdeaThemeLink> SMIdeaThemeLinks { get; set; }
        public virtual DbSet<IdeaThemeType> SMIdeaThemeTypes { get; set; }
        public virtual DbSet<ValueProposition> SMValuePropositions { get; set; }
        public virtual DbSet<AgeRange> SMAgeRanges { get; set; }
        public virtual DbSet<Area> SMAreas { get; set; }
        public virtual DbSet<CriteriaValue> SMCriteriaValues { get; set; }
        public virtual DbSet<CustomCriteriaDefinition> SMCustomCriteriaDefinitions { get; set; }
        public virtual DbSet<CustomOption> SMCustomOptions { get; set; }
        public virtual DbSet<Place> SMPlaces { get; set; }
        public virtual DbSet<PlaceActivity> SMPlaceActivitys { get; set; }
        public virtual DbSet<Segment> SMSegments { get; set; }
        public virtual DbSet<SegmentQuery> SMSegmentQuerys { get; set; }
        public virtual DbSet<SegmentQueryClause> SMSegmentQueryClauses { get; set; }
        public virtual DbSet<SMContact> SMContacts { get; set; }
        public virtual DbSet<Visit> SMVisits { get; set; }
        public virtual DbSet<VisitActivityLog> SMVisitActivityLogs { get; set; }
        public virtual DbSet<ReminderQueue> ReminderQueues { get; set; }
        public virtual DbSet<Pipeline> Pipelines { get; set; }
        public virtual DbSet<PipelineContact> PipelineContacts { get; set; }
        public virtual DbSet<Step> PipelineSteps { get; set; }
        public virtual DbSet<EmailTemplate> EmailTemplates { get; set; }

        #endregion Sales and Marketing

        #region Trader Sale Return

        public virtual DbSet<TraderReturn> TraderReturns { get; set; }
        public virtual DbSet<ReturnLog> TraderReturnLogs { get; set; }
        public virtual DbSet<ReturnItem> TradeReturnItems { get; set; }
        public virtual DbSet<ReturnItemLog> ReturnItemLogs { get; set; }
        public virtual DbSet<ReturnProcessLog> ReturnProcessLogs { get; set; }

        #endregion Trader Sale Return

        #region Spannered

        public virtual DbSet<Asset> SpanneredAssets { get; set; }
        public virtual DbSet<AssetTag> SpanneredTags { get; set; }
        public virtual DbSet<Meter> SpanneredMeters { get; set; }
        public virtual DbSet<MeterLog> SpanneredMeterLogs { get; set; }
        public virtual DbSet<SpanneredProcess> SpanneredProcesses { get; set; }
        public virtual DbSet<SpanneredWorkgroup> SpanneredWorkgroups { get; set; }
        public virtual DbSet<AssetInventory> SpanneredAssetInventories { get; set; }
        public virtual DbSet<ConsumptionReport> SpanneredConsumptionReports { get; set; }
        public virtual DbSet<ConsumptionReportLog> SpanneredConsumptionReportLogs { get; set; }
        public virtual DbSet<ConsumptionReportProcessLog> SpanneredConsumptionReportProcessLogs { get; set; }
        public virtual DbSet<ConsumptionItem> SpanneredConsumptionItems { get; set; }
        public virtual DbSet<ConsumptionItemLog> SpanneredConsumptionItemLogs { get; set; }
        public virtual DbSet<ConsumablesPartServiceItem> SpanneredTaskConsumablesPartServiceItems { get; set; }
        public virtual DbSet<ConsumeReportApprovalDefinition> ConsumeReportApprovalDefinitions { get; set; }

        #endregion Spannered

        #region Operator

        public virtual DbSet<OperatorSetting> OperatorSettings { get; set; }
        public virtual DbSet<OperatorWorkGroup> OperatorWorkGroups { get; set; }
        public virtual DbSet<WorkGroupTaskMember> OperatorWGTaskMembers { get; set; }
        public virtual DbSet<WorkGroupTeamMember> OperatorWGTeamMembers { get; set; }
        public virtual DbSet<OperatorTag> OperatorTags { get; set; }
        public virtual DbSet<OperatorLocation> OperatorLocations { get; set; }
        public virtual DbSet<OperatorRole> OperatorRoles { get; set; }
        public virtual DbSet<ComplianceForms> OperatorComplianceForms { get; set; }
        public virtual DbSet<ComplianceTask> OperatorComplianceTasks { get; set; }
        public virtual DbSet<OrderedForm> OperatorOrderedForms { get; set; }
        public virtual DbSet<TaskInstance> OperatorTaskInstances { get; set; }
        public virtual DbSet<Goal> OperatorGoals { get; set; }
        public virtual DbSet<GoalMeasure> OperatorGoalMeasures { get; set; }
        public virtual DbSet<Measure> OperatorMeasures { get; set; }
        public virtual DbSet<PerformanceTracking> OperatorPerformanceTrackings { get; set; }
        public virtual DbSet<TeamPerson> OperatorTeamPersons { get; set; }
        public virtual DbSet<TrackingMeasure> OperatorTrackingMeasures { get; set; }
        public virtual DbSet<Attendance> OperatorAttendances { get; set; }
        public virtual DbSet<Schedule> OperatorSchedules { get; set; }

        #endregion Operator

        #region B2B

        public virtual DbSet<B2BProfile> B2BProfiles { get; set; }
        public virtual DbSet<AreaOfOperation> B2BAreasOfOperation { get; set; }
        public virtual DbSet<B2BPost> B2BPosts { get; set; }
        public virtual DbSet<B2BQbicle> B2BQbicles { get; set; }
        public virtual DbSet<ExchangeRate> ExchangeRates { get; set; }
        public virtual DbSet<B2BRelationship> B2BRelationships { get; set; }
        public virtual DbSet<Partnership> B2BPartnerships { get; set; }
        public virtual DbSet<LogisticsPartnership> B2BLogisticsPartnerships { get; set; }
        public virtual DbSet<ChargeFramework> B2BChargeFrameworks { get; set; }
        public virtual DbSet<PriceList> B2BPriceLists { get; set; }
        public virtual DbSet<Vehicle> B2BVehicles { get; set; }
        public virtual DbSet<B2BTradingItem> B2BTradingItems { get; set; }
        public virtual DbSet<B2BCatalogItem> B2BCatalogItems { get; set; }
        public virtual DbSet<TradeOrderB2B> B2BTradeOrders { get; set; }
        public virtual DbSet<B2BPartnershipDiscussion> B2BPartnershipDiscussions { get; set; }
        public virtual DbSet<PurchaseSalesPartnership> B2BPurchaseSalesPartnerships { get; set; }
        public virtual DbSet<B2BProviderPriceList> B2BProviderPriceLists { get; set; }
        public virtual DbSet<B2BProviderChargeFramework> B2BProviderChargeFrameworks { get; set; }
        public virtual DbSet<B2BLogisticsAgreement> B2BLogisticsAgreements { get; set; }
        public virtual DbSet<B2BSocialLink> B2BSocialLinks { get; set; }
        public virtual DbSet<B2BTag> B2BTags { get; set; }
        public virtual DbSet<B2BOrderCreation> B2BOrderCreations { get; set; }

        #endregion B2B

        #region MyBankMate

        public virtual DbSet<DriverBankmateAccount> DriverBankmateAccounts { get; set; }
        public virtual DbSet<ExternalBankAccount> ExternalBankAccounts { get; set; }
        public virtual DbSet<Bank> Banks { get; set; }

        #endregion MyBankMate

        #region MovementAlert

        public virtual DbSet<AccumulationEntryReport> AccumulationEntryReports { get; set; }
        public virtual DbSet<AlertConstraint> AlertConstraints { get; set; }
        public virtual DbSet<AlertGroup> AlertGroups { get; set; }
        public virtual DbSet<Qbicles.Models.Trader.Movement.DateRange> DateRanges { get; set; }
        public virtual DbSet<Item_AlertGroup_Xref> Item_AlertGroup_Xrefs { get; set; }
        public virtual DbSet<MinMaxInventoryReportEntry> MinMaxInventoryReportEntries { get; set; }
        public virtual DbSet<NoMovementReportEntry> NoMovementReportEntries { get; set; }
        public virtual DbSet<Report> Reports { get; set; }
        public virtual DbSet<ReportEntry> ReportEntries { get; set; }

        #endregion MovementAlert

        #region B2C And C2C

        public virtual DbSet<CQbicle> CQbicles { get; set; }
        public virtual DbSet<B2CQbicle> B2CQbicles { get; set; }
        public virtual DbSet<C2CQbicle> C2CQbicles { get; set; }
        public virtual DbSet<CustomerRelationshipLog> CustomerRelationshipLogs { get; set; }
        public virtual DbSet<B2COrderCreation> B2COrderCreations { get; set; }
        public virtual DbSet<B2CProductMenuDiscussion> B2CProductMenuDiscussions { get; set; }
        public virtual DbSet<B2BCatalogDiscussion> B2BCatalogDiscussions { get; set; }
        public virtual DbSet<TradeOrder> TradeOrders { get; set; }
        public virtual DbSet<B2COrderPaymentCharges> B2COrderPaymentCharges { get; set; }

        #endregion B2C And C2C

        #region HighlightPost

        public virtual DbSet<HighlightPost> HighlightPosts { get; set; }
        public virtual DbSet<UserAndHighlightPostXref> UserAndHighlightPostXrefs { get; set; }
        public virtual DbSet<NewsHighlight> NewsHighlightPosts { get; set; }
        public virtual DbSet<ArticleHighlight> ArticleHighlightPosts { get; set; }
        public virtual DbSet<KnowledgeHighlight> KnowledgeHighlightPosts { get; set; }
        public virtual DbSet<ListingHighlight> ListingHighlightPosts { get; set; }
        public virtual DbSet<JobListingHighlight> JobHighlightPosts { get; set; }
        public virtual DbSet<EventListingHighlight> EventHighlightPosts { get; set; }
        public virtual DbSet<RealEstateListingHighlight> RealEstateHighlightPosts { get; set; }
        public virtual DbSet<PropertyType> PropertyTypes { get; set; }
        public virtual DbSet<PropertyExtras> PropertyExtras { get; set; }
        public virtual DbSet<HighlightLocation> HighlightLocations { get; set; }
        public virtual DbSet<HLSharedPost> HLSharedPosts { get; set; }
        public virtual DbSet<LoyaltySharedPromotion> SharedPromotions { get; set; }

        #endregion HighlightPost

        #region Sale Channel Settings

        public virtual DbSet<B2CSettings> B2CSettings { get; set; }
        public virtual DbSet<B2BSettings> B2BSettings { get; set; }
        public virtual DbSet<PosSettings> PosSettings { get; set; }
        //public virtual DbSet<StoreFrontSettings> StoreFrontSettings { get; set; }

        #endregion Sale Channel Settings

        #region User Information

        public virtual DbSet<Showcase> Showcases { get; set; }
        public virtual DbSet<Models.UserInformation.Skill> Skills { get; set; }
        public virtual DbSet<Experience> Experiences { get; set; }
        public virtual DbSet<UserProfileFile> UserProfileFiles { get; set; }
        public virtual DbSet<WorkExperience> WorkExperiences { get; set; }
        public virtual DbSet<EducationExperience> EducationExperiences { get; set; }
        public virtual DbSet<TempEmailAddress> TempEmailAddress { get; set; }

        #endregion User Information

        #region Network

        public virtual DbSet<ShortListGroup> ShortListGroups { get; set; }

        #endregion Network

        #region Profile Page

        public virtual DbSet<ProfilePage> ProfilePages { get; set; }
        public virtual DbSet<BusinessPage> BusinessPages { get; set; }
        public virtual DbSet<UserPage> UserPages { get; set; }
        public virtual DbSet<Block> BlockTemplates { get; set; }
        public virtual DbSet<Hero> BlockHeroTemplates { get; set; }
        public virtual DbSet<FeatureList> BlockFeatureTemplates { get; set; }
        public virtual DbSet<FeatureItem> BlockFeatureItems { get; set; }
        public virtual DbSet<GalleryList> BlockGalleryTemplates { get; set; }
        public virtual DbSet<GalleryItem> BlockGalleryItems { get; set; }
        public virtual DbSet<Models.ProfilePage.Promotion> BlockPromotionTemplates { get; set; }
        public virtual DbSet<PromotionItem> BlockPromotionItems { get; set; }
        public virtual DbSet<TestimonialList> BlockTestimonialTemplates { get; set; }
        public virtual DbSet<TestimonialItem> BlockTestimonialItems { get; set; }
        public virtual DbSet<TextImage> BlockTextImageTemplates { get; set; }
        public virtual DbSet<MasonryGallery> BlockMasonryGalleryTemplates { get; set; }
        public virtual DbSet<MasonryGalleryItem> BlockMasonryGalleryItems { get; set; }
        public virtual DbSet<HeroPersonal> BlockHeroPersonalTemplates { get; set; }

        #endregion Profile Page

        #region Loyalty

        public virtual DbSet<OrderToPointsConversion> OrderToPointsConversions { get; set; }
        public virtual DbSet<PaymentConversion> PaymentConversions { get; set; }
        public virtual DbSet<StorePointAccount> StorePointAccounts { get; set; }
        public virtual DbSet<StorePointTransaction> StorePointTransactions { get; set; }
        public virtual DbSet<StorePointTransactionLog> StorePointTransactionLogs { get; set; }
        public virtual DbSet<SystemSettings> LoyaltySystemSettings { get; set; }
        public virtual DbSet<DomainLoyaltySettings> DomainLoyaltySettings { get; set; }
        public virtual DbSet<StoreCreditAccount> StoreCreditAccounts { get; set; }
        public virtual DbSet<StoreCreditTransaction> StoreCreditTransactions { get; set; }
        public virtual DbSet<StoreCredit> StoreCredits { get; set; }
        public virtual DbSet<StoreCreditPIN> StoreCreditPINs { get; set; }
        public virtual DbSet<LoyaltyPromotion> Promotions { get; set; }
        public virtual DbSet<LoyaltyBulkDealPromotion> BulkDealPromotions { get; set; }
        public virtual DbSet<LoyaltyPromotionType> PromotionTypes { get; set; }
        public virtual DbSet<LoyaltyPromotionAudience> PromotionAudiences { get; set; }
        public virtual DbSet<LoyaltyPromotionPaymentTransaction> PromotionPaymentTransactions { get; set; }
        public virtual DbSet<Voucher> Vouchers { get; set; }
        public virtual DbSet<BulkDealVoucher> BulkDealVouchers { get; set; }
        public virtual DbSet<VoucherInfo> VoucherInfos { get; set; }
        public virtual DbSet<BulkDealVoucherInfo> BulkDealVoucherInfos { get; set; }
        public virtual DbSet<ItemDiscountVoucherInfo> ItemDiscountVoucherInfos { get; set; }
        public virtual DbSet<ItemDiscountBulkDealVoucherInfo> ItemDiscountBulkDealVoucherInfos { get; set; }
        public virtual DbSet<OrderDiscountVoucherInfo> OrderDiscountVoucherInfos { get; set; }
        public virtual DbSet<OrderDiscountBulkDealVoucherInfo> OrderDiscountBulkDealVoucherInfos { get; set; }
        public virtual DbSet<LoyaltyWeekDay> LoyaltyWeekDays { get; set; }
        public virtual DbSet<LoyaltyBulkDealWeekDay> LoyaltyBulkDealWeekDays { get; set; }
        public virtual DbSet<StoreDebit> StoreDebits { get; set; }

        #endregion Loyalty

        #region Trader order log event

        public virtual DbSet<DeliveryStatusUpdate> DeliveryStatusUpdates { get; set; }
        public virtual DbSet<PreparationStatusUpdate> PreparationStatusUpdates { get; set; }
        public virtual DbSet<InvoiceAdded> InvoiceAddeds { get; set; }
        public virtual DbSet<InvoiceApprovalStatusUpdate> InvoiceApprovalStatusUpdates { get; set; }
        public virtual DbSet<PaymentAdded> PaymentAddeds { get; set; }
        public virtual DbSet<PaymentApprovalStatusUpdate> PaymentApprovalStatusUpdates { get; set; }
        public virtual DbSet<PurchaseAdded> PurchaseAddeds { get; set; }
        public virtual DbSet<PurchaseApprovalStatusUpdate> PurchaseApprovalStatusUpdates { get; set; }
        public virtual DbSet<SaleAdded> SaleAddeds { get; set; }
        public virtual DbSet<SaleApprovalStatusUpdate> SaleApprovalStatusUpdates { get; set; }
        public virtual DbSet<TransferAdded> TransferAddeds { get; set; }
        public virtual DbSet<TransferApprovalStatusUpdate> TransferApprovalStatusUpdates { get; set; }

        #endregion Trader order log event

        #region Simple Email Service

        public virtual DbSet<SESIdentity> SESIdentities { get; set; }

        #endregion Simple Email Service

        #region PaystackSubscriptionPayments

        public virtual DbSet<PaystackSubscriptionPayment> PaystackSubscriptionPayments { get; set; }

        #endregion PaystackSubscriptionPayments

        #region Waitlist

        public virtual DbSet<WaitListRequest> WaitListRequests { get; set; }
        public virtual DbSet<DomainCreationRights> DomainCreationRights { get; set; }
        public virtual DbSet<DomainCreationRightsLog> DomainCreationRightsLogs { get; set; }

        #endregion Waitlist

        #region Category Exclusion

        public virtual DbSet<CategoryExclusionSet> CategoryExclusionSets { get; set; }
        public virtual DbSet<ExclusionCategoryName> ExclusionCategoryNames { get; set; }

        #endregion Category Exclusion

        #region Firebase

        public virtual DbSet<FirebaseNotificationToken> FirebaseNotificationTokens { get; set; }

        #endregion Firebase
    }
}