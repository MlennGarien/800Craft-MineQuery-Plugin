using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;
using fCraft;

namespace MineQuery
{
    class MineQueryServer
    {
        private static MineQueryServer INSTANCE;
        private Timer timer;
        private TcpListener listener;
        private static readonly object syncRoot = new object();

        private MineQueryServer()
        {
            listener = new TcpListener(IPAddress.Any, Init.Config.MineQueryPort);
            listener.Start();

            Logger.Log(LogType.SystemActivity, "Started MineQuery server on port " + Init.Config.MineQueryPort);

            timer = new Timer(250);
            timer.Elapsed += new ElapsedEventHandler(CheckClients);
            timer.Start();
        }

        private void CheckClients(object sender, ElapsedEventArgs e)
        {
            lock (syncRoot)
            {
                if (listener.Pending())
                {
                    NetworkStream clientStream = null;
                    StreamReader clientReader = null;
                    TcpClient client = null;

                    try
                    {
                        client = listener.AcceptTcpClient();

                        if (client != null)
                        {
                            clientStream = client.GetStream();
                            clientReader = new StreamReader(clientStream);
                            clientStream.ReadTimeout = 10000;

                            Logger.Log(LogType.SystemActivity, "MineQuery client connected from " + client.Client.RemoteEndPoint.ToString());

                            String request = clientReader.ReadLine();
                            byte[] dataSend = Encoding.UTF8.GetBytes("Invalid query");

                            if (request.ToUpper().Replace(Environment.NewLine, String.Empty).Equals("QUERY"))
                            {
                                StringBuilder dataAssemble = new StringBuilder();
                                dataAssemble.AppendLine("SERVERPORT " + ConfigKey.Port.GetInt());
                                dataAssemble.AppendLine("PLAYERCOUNT " + Server.Players.Length);
                                dataAssemble.AppendLine("MAXPLAYERS " + ConfigKey.MaxPlayers.GetString());

                                if (Server.Players.Length > 0)
                                {
                                    Player[] players = Server.Players;
                                    String[] playerNames = new String[players.Length];

                                    for (int i = 0; i < players.Length; i++)
                                    {
                                        playerNames[i] = players[i].Name;
                                    }

                                    dataAssemble.AppendLine("PLAYERLIST [" + String.Join(",", playerNames) + "]");
                                }
                                else
                                {
                                    dataAssemble.AppendLine("PLAYERLIST []");
                                }

                                dataSend = Encoding.UTF8.GetBytes(dataAssemble.ToString());
                                clientStream.Write(dataSend, 0, dataSend.Length);
                            }
                            else if (request.ToUpper().Replace(Environment.NewLine, String.Empty).Equals("QUERY_JSON"))
                            {
                                MineQueryResponse response = new MineQueryResponse()
                                {
                                    serverPort = Server.Port,
                                    maxPlayers = ConfigKey.MaxPlayers.GetInt(),
                                    playerCount = Server.Players.Length,
                                    playerList = new List<string>()
                                };

                                if (Server.Players.Length > 0)
                                {
                                    Player[] players = Server.Players;

                                    for (int i = 0; i < players.Length; i++)
                                    {
                                        response.playerList.Add(players[i].Name);
                                    }
                                }

                                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(MineQueryResponse));
                                serializer.WriteObject(clientStream, response);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogType.Error, "Unable to execute MineQuery: " + ex);
                    }
                    finally
                    {
                        try
                        {
                            clientStream.Close();
                            client.Close();

                        }
                        catch (Exception ex)
                        {
                            Logger.Log(LogType.Error, "Unable to close MineQuery network stream: " + ex);
                        }
                    }
                }
            }
        }

        public static MineQueryServer GetInstance()
        {
            INSTANCE = new MineQueryServer();

            return INSTANCE;
        }
    }
}