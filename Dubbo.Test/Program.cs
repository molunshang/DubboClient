using Dubbo.Attribute;
using Dubbo.Config;
using Dubbo.Registry;
using Hessian.Lite.Attribute;
using Hessian.Lite.Util;
using Rabbit.Zookeeper;
using Rabbit.Zookeeper.Implementation;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Dubbo.Test
{
    class Program
    {
        static void TestDubboInvoke()
        {
            //var request = new Request
            //{
            //    MethodName = "getLoanById",
            //    ParameterTypeInfo = "Ljava/lang/String;",
            //    Arguments = new[] { "20180328_86F1_406E_B86B_EE6F0D09D98E" },
            //    Attachments = new Dictionary<string, string>()
            //};
            //request.Attachments["path"] = "com.fengjr.fengchu.dubbo.api.LoanService";
            //request.Attachments["interface"] = "com.fengjr.fengchu.dubbo.api.LoanService";
            ////            request.Attachments["version"] = "1.0.0";
            //request.Attachments["timeout"] = "100000";

            //var client = new TcpClient();
            //client.Connect(IPAddress.Parse("10.254.21.59"), 20880);

            //var channel = client.GetStream();
            //Codec.EncodeRequest(request, channel);
            //channel.Flush();
            //var resHeader = new byte[16];
            //var size = channel.Read(resHeader, 0, 16);
            //while (size < 16)
            //{
            //    size += channel.Read(resHeader, 0, 16 - size);
            //}

            //Console.WriteLine(resHeader.ReadLong(4));
            //var bodyLength = resHeader.ReadInt(12);
            //var body = new byte[bodyLength];
            //size = channel.Read(body, 0, bodyLength);
            //while (size < bodyLength)
            //{
            //    size += channel.Read(body, size, bodyLength - size);
            //}

            //var stream = new MemoryStream(body);
            //var input = new Hessian2Reader(stream);
            //var flag = input.ReadObject();
            //Console.WriteLine(flag);
        }

        static void InvokeDubbo(ServiceConfig config)
        {
            //var request = new Request
            //{
            //    MethodName = "sayHello",
            //    ParameterTypeInfo = "Ljava/lang/String;",
            //    Arguments = new[] { "invoke from .net client" },
            //    Attachments = new Dictionary<string, string>(),
            //    IsTwoWay = true
            //};
            //request.Attachments["path"] = "org.apache.dubbo.demo.DemoService";
            //request.Attachments["interface"] = "org.apache.dubbo.demo.DemoService";
            ////            request.Attachments["version"] = "1.0.0";
            //request.Attachments["timeout"] = "100000";
            //var connection = new Connection(config.Host, config.Port);
            //connection.Connect().ContinueWith(t =>
            //{
            //    if (t.IsCompletedSuccessfully)
            //    {
            //        connection.Send(request);
            //        return;
            //    }
            //    Console.WriteLine(t.Exception);
            //});
        }

        class TestProxy : DispatchProxy
        {
            protected override object Invoke(MethodInfo targetMethod, object[] args)
            {
                Console.WriteLine("Invoke Method " + targetMethod.Name);
                return targetMethod.ReturnType.Default();
            }
        }

        public enum Source
        {
            MOBILEWEB,
            WECHAT,
            WEB,
            BACK,
            MOBILE,
            IOS,
            ANDROID,
            BATCH,
            UNKNOW
        }

        public enum Realm
        {
            STRING,
            KEYVALUE,
            SHADOW_BORROWER,
            LOAN_TYPE,
            LOAN_ADDTIONAL_RATE,
            LEGAL_PERSON,
            FINANCE_CORPORATION,
            USER,
            CORPORATIONUSER,
            EMPLOYEE,
            ROLE,
            CLIENT,
            BRANCH,
            CORPORATION,
            USER_CORPORATION,
            BANK,
            PROOF,
            CERTIFICATE,
            VEHICLE,
            REALESTATE,
            INVEST,
            LOAN,
            CREDITASSIGN,
            CREDITASSIGNINVEST,
            INVESTREPAYMENT,
            LOANREPAYMENT,
            LOANREQUEST,
            TASK,
            APPOINTMENT,
            FUND,
            WITHDRAW,
            TRANSFER,
            CHANNEL,
            ARTICLE,
            STATISTICS,
            INSTATIONMESSAGE,
            CMS_MANAGER,
            FUNDINGPROJECT,
            FUNDINGDREAM,
            FUNDINGINVEST,
            PROJECTLOAN,
            FUNDINGREWARD,
            FUNDINGLIKE,
            FUNDINGSPACIALTOPIC,
            FUNDINGXIAOHUAJIA,
            MOBILE,
            IMAGE,
            FILE,
            FSS,
            TICKET,
            BATCHJOB,
            PAGES,
            BATCH,
            COUPON,
            CLAIM,
            CONTRACT,
            CONTRACTSEAL,
            CONTRACTTEMPLATE,
            CONTRACTTEMPLATE_O2O,
            CONTRACTTEMPLATE_O2M,
            BROKERAGE_CONTRACTTEMPLATE,
            FUNDING_CONTRACTTEMPLATE,
            ASSIGN_CONTRACTTEMPLATE,
            SERIALNUMBER,
            GUARANTEE_AUDIT,
            ORDER,
            SYSTEM,
            CMS_ARTICLE_FAVORITE,
            LOAN_RECEIPTY,
            CHANNEL_PRODUCT
        }

        [DubboType("com.fengjr.usercenter.model.User")]
        public class RealmEntity
        {
            [Name("realm")]
            public Realm Realm { get; set; }

            [Name("entityId")]
            public string EntityId { get; set; }
        }

        [DubboType("com.fengjr.usercenter.model.User")]
        public class User
        {
            [Name("id")]
            public string Id { get; set; }
            [Name("name")]
            public string Name { get; set; }
            [Name("loginName")]
            public string LoginName { get; set; }
            [Name("idNumber")]
            public string IdNumber { get; set; }
            [Name("mobile")]
            public string Mobile { get; set; }
            [Name("email")]
            public string Email { get; set; }
            [Name("source")]
            public Source Source { get; set; }
            [Name("employeeId")]
            public string EmployeeId { get; set; }
            [Name("lastModifiedBy")]
            public string LastModifiedBy { get; set; }
            [Name("channel")]
            public string Channel { get; set; }
            [Name("lastLoginDate")]
            public DateTime LastLoginDate { get; set; }
            [Name("registerDate")]
            public DateTime RegisterDate { get; set; }
            [Name("enabled")]
            public bool Enabled { get; set; }
            [Name("referralEntity")]
            public RealmEntity ReferralEntity { get; set; }
            [Name("enterprise")]
            public bool Enterprise { get; set; }
            [Name("registryRewarded")]
            public bool RegistryRewarded { get; set; }
            [Name("referralRewarded")]
            public bool ReferralRewarded { get; set; }
            [Name("inviteCode")]
            public string InviteCode { get; set; }
        }

        [DubboService(TargetService = "com.fengjr.usercenter.api.UserService", Version = "2.0.1")]
        public interface IUserService
        {
            [DubboMethod("listByUserIds")]
            List<User> ListByUserIds(List<string> userIds);
            [DubboMethod("listByUserIds")]
            Task<List<User>> ListByUserIdsAsync(List<string> userIds);
        }

        [DubboService(TargetService = "org.apache.dubbo.demo.DemoService")]
        public interface ITest
        {
            [DubboMethod("sayHello")]
            string Hello(string arg);

            void Invoke<T>(T t);
        }

        static void Main(string[] args)
        {
            IZookeeperClient zookeeper = new ZookeeperClient(new ZookeeperClientOptions()
            {
                ConnectionString = "bzk1.fengjr.inc:2181,bzk2.fengjr.inc:2181,bzk3.fengjr.inc:2181",
                ConnectionTimeout = TimeSpan.FromSeconds(30)
            });
            var factory = new DubboClientFactory(new ZookeeperRegistry(zookeeper));
            var test = factory.CreateClient<IUserService>();
            var ids = new List<string> { "8440073C-2A95-4BF6-960B-03929CF98021", "2AF36CBF-9DEA-4D9B-81F1-310AEE3D832E" };
            test.ListByUserIdsAsync(ids).ContinueWith(t =>
            {
                Console.WriteLine(t.Result);
            });
            Console.ReadKey();
        }
    }
}