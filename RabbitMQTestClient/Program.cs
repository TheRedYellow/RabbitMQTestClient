using System;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;


using RabbitMQ.Client;


namespace RabbitMQTestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.Write("İstemci türünü giriniz [P => Producer/ C => Consumer] : ");
                var clientType = Console.ReadLine();


                if (clientType == null || (clientType.ToLower() != "p" && clientType.ToLower() != "c"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Lütfen istemci türü için p ya da c değeri giriniz...");
                    Console.ResetColor();
                }
                else
                {
                    clientType = clientType.ToLower();


                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("----------------------------------------------");
                    if (clientType == "p")
                    {
                        Console.WriteLine("Uygulama Producer modunda başlatılıyor");
                    }
                    else
                    {
                        Console.WriteLine("Uygulama Consumer modunda başlatılıyor");
                    }
                    Console.WriteLine("----------------------------------------------");
                    Console.ResetColor();


                    Console.Write("Kullanıcı adını giriniz : ");
                    var userName = Console.ReadLine();


                    Console.Write("Kullanıcı şifresi giriniz : ");
                    var password = Console.ReadLine();


                    Console.Write("RabbitMQ Virtual Host giriniz : ");
                    var virtualHost = Console.ReadLine();


                    Console.Write("RabbitMQ sunucu adresini giriniz : ");
                    var hostName = Console.ReadLine();


                    Console.Write("Routing key'i giriniz : ");
                    var routingKey = Console.ReadLine();


                    var queueName = string.Empty;
                    var testMessage = string.Empty;
                    var exchangeName = string.Empty;
                    var messageCount = 0;


                    if (clientType == "c")
                    {
                        Console.Write("Kuyruk adını giriniz : ");
                        queueName = Console.ReadLine();


                        Console.Write("okunacak mesaj sayısını giriniz : ");
                        messageCount = int.Parse(Console.ReadLine());
                    }


                    if (clientType == "p")
                    {
                        Console.Write("Exchange adını giriniz : ");
                        exchangeName = Console.ReadLine();


                        Console.Write("Gönderilecek mesaj sayısını giriniz : ");
                        messageCount = int.Parse(Console.ReadLine());


                        Console.Write("Mesajı giriniz : ");
                        testMessage = Console.ReadLine();
                    }


                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("-----------------------------------");
                    Console.WriteLine($"Kullanıcı Adı: {userName}");
                    Console.WriteLine($"Şifre : {password}");
                    Console.WriteLine($"RabbitMQ Virtual Host: {virtualHost}");
                    Console.WriteLine($"Sunucu Ip: {hostName}");
                    Console.WriteLine($"Routing Key: {routingKey}");
                    if (clientType == "c")
                    {
                        Console.WriteLine($"Kuyruk Adı: {queueName}");
                    }
                    Console.WriteLine($"Mesaj Sayısı: {messageCount}");
                    if (clientType == "p")
                    {
                        Console.WriteLine($"Exchange Adı: {exchangeName}");
                        Console.WriteLine($"Test Mesajı: {testMessage}");
                    }
                    Console.WriteLine("-----------------------------------");
                    Console.ResetColor();




                    Console.WriteLine("» RabbitMQ bağlantısı oluşturuluyor");
                    ConnectionFactory factory = new ConnectionFactory();
                    factory.UserName = userName;
                    factory.Password = password;
                    factory.VirtualHost = virtualHost;


                    factory.HostName = hostName;
                    factory.Port = AmqpTcpEndpoint.DefaultAmqpSslPort;
                    factory.Ssl.Enabled = true;
                    factory.Ssl.Version = System.Security.Authentication.SslProtocols.Tls12;
                    //factory.Ssl.AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateChainErrors | SslPolicyErrors.RemoteCertificateNotAvailable;


                    factory.Ssl.CertificateValidationCallback += RemoteCertificateValidationCallback;
                    Console.WriteLine("» RabbitMQ bağlantısı açılıyor");


                    using (IConnection conn = factory.CreateConnection())
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("√ RabbitMQ sunucusuna bağlanıldı");
                        Console.ResetColor();


                        Console.WriteLine("» RabbitMQ kanal açılıyor");
                        using (IModel ch = conn.CreateModel())
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("√ RabbitMQ kanalı açıldı");
                            Console.ResetColor();


                            if (clientType == "p")
                            {
                                for (int i = 0; i < messageCount; i++)
                                {
                                    Console.WriteLine($"» {i + 1}. mesaj kuyruğa bırakılıyor");
                                    ch.BasicPublish(exchangeName, routingKey, null, Encoding.UTF8.GetBytes($"{i} => {testMessage}"));


                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("√ RabbitMQ kuyruğuna mesaj bırakıl");
                                    Console.ResetColor();
                                }
                            }
                            else
                            {
                                Console.WriteLine("» Kuyruktan mesajlar okunuyor");


                                for (int i = 0; i < messageCount; i++)
                                {
                                    Console.WriteLine($"» {i + 1}. mesaj kuyruktan okunuyor");
                                    BasicGetResult result = ch.BasicGet(queueName, true);


                                    if (result == null)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine($"{i + 1}. mesaj kuyruktan okunamadı");
                                        Console.ResetColor();
                                    }
                                    else
                                    {
                                        var messageContent = Encoding.UTF8.GetString(result.Body.ToArray());
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"√ {i + 1}. mesaj kuyruktan okundu: {messageContent}");
                                        Console.ResetColor();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("-----------------------------------");
                Console.WriteLine($"» Test sırasında beklenmedik bir hata oluştu: {ex}");
                Console.WriteLine("-----------------------------------");
                Console.ResetColor();
            }


            Console.ResetColor();
            Console.WriteLine("Uygulama sonlandı. Çıkış için herhangi bir tuşa basınız...");
            Console.ReadKey();
        }


        public static bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            Console.WriteLine("Sertifika doğrulanacak");
            Console.WriteLine("Sertifika hash'i : {0}", certificate.GetCertHashString());
            Console.WriteLine("Sertifika sağlayıcı : {0}", certificate.Issuer);
            Console.WriteLine("Sertifika Adı : {0}", certificate.Subject);
            Console.WriteLine("Sertifika Seri numarası : {0}", certificate.GetSerialNumberString());


            return true;
        }

    }
}
