﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.cliPackets;
using wServer.svrPackets;
using wServer.realm.setpieces;

namespace wServer.realm.entities
{
    partial class Player
    {
        public void PlayerText(RealmTime time, PlayerTextPacket pkt)
        {
            if (pkt.Text[0] == '/')
            {
                string[] x = pkt.Text.Trim().Split(' ');
                ProcessCmd(x[0].Trim('/'), x.Skip(1).ToArray());
            }
            else
                Owner.BroadcastPacket(new TextPacket()
                {
                    Name = Name,
                    ObjectId = Id,
                    Stars = Stars,
                    BubbleTime = 5,
                    Recipient = "",
                    Text = pkt.Text,
                    CleanText = pkt.Text
                }, null);
        }

        bool CmdReqAdmin()
        {

            if (!psr.Account.Admin)
            {
                psr.SendPacket(new TextPacket()
                {
                    BubbleTime = 0,
                    Stars = -1,
                    Name = "",
                    Text = "You are not an Admin!"
                });
                return false;
            }
            else

                return true;
        }
        void ProcessCmd(string cmd, string[] args)
        {
            if (cmd.Equals("tutorial", StringComparison.OrdinalIgnoreCase))
                psr.Reconnect(new ReconnectPacket()
                {
                    Host = "",
                    Port = 2050,
                    GameId = World.TUT_ID,
                    Name = "Tutorial",
                    Key = Empty<byte>.Array,
                });
            else if (cmd.Equals("spawn", StringComparison.OrdinalIgnoreCase) &&
                     CmdReqAdmin() && args.Length > 0)
            {
                string name = string.Join(" ", args);
                short objType;
                if (!XmlDatas.IdToType.TryGetValue(name, out objType) ||
                    !XmlDatas.ObjectDescs.ContainsKey(objType))
                    psr.SendPacket(new TextPacket()
                    {
                        BubbleTime = 0,
                        Stars = -1,
                        Name = "",
                        Text = "Unknown entity!"
                    });
                else
                {
                    var entity = Entity.Resolve(objType);
                    entity.Move(X, Y);
                    Owner.EnterWorld(entity);
                    psr.SendPacket(new TextPacket()
                    {
                        BubbleTime = 0,
                        Stars = -1,
                        Name = "",
                        Text = "Success!"
                    });
                }
            }
            else if (cmd.Equals("realm", StringComparison.OrdinalIgnoreCase))
                psr.Reconnect(new ReconnectPacket()
                {
                    Host = "",
                    Port = 2050,
                    GameId = World.RAND_REALM,
                    Name = "Realm",
                    Key = Empty<byte>.Array,
                });
            else if (cmd.Equals("spawnx", StringComparison.OrdinalIgnoreCase) &&
                     CmdReqAdmin() && args.Length > 1)
            {
                string name = string.Join(" ", args.Skip(1).ToArray());
                short objType;
                if (!XmlDatas.IdToType.TryGetValue(name, out objType) ||
                    !XmlDatas.ObjectDescs.ContainsKey(objType))
                    psr.SendPacket(new TextPacket()
                    {
                        BubbleTime = 0,
                        Stars = -1,
                        Name = "",
                        Text = "Unknown entity!"
                    });
                else
                {
                    int c = int.Parse(args[0]);
                    for (int i = 0; i < c; i++)
                    {
                        var entity = Entity.Resolve(objType);
                        entity.Move(X, Y);
                        Owner.EnterWorld(entity);
                    }
                    psr.SendPacket(new TextPacket()
                    {
                        BubbleTime = 0,
                        Stars = -1,
                        Name = "",
                        Text = "Success!"
                    });
                }
            }
            else if (cmd.Equals("addEff", StringComparison.OrdinalIgnoreCase) &&
                     CmdReqAdmin() && args.Length == 1)
            {
                try
                {
                    ApplyConditionEffect(new ConditionEffect()
                    {
                        Effect = (ConditionEffectIndex)Enum.Parse(typeof(ConditionEffectIndex), args[0].Trim()),
                        DurationMS = -1
                    });
                    psr.SendPacket(new TextPacket()
                    {
                        BubbleTime = 0,
                        Stars = -1,
                        Name = "",
                        Text = "Success!"
                    });
                }
                catch
                {
                    psr.SendPacket(new TextPacket()
                    {
                        BubbleTime = 0,
                        Stars = -1,
                        Name = "",
                        Text = "Invalid effect!"
                    });
                }
            }
            else if (cmd.Equals("removeEff", StringComparison.OrdinalIgnoreCase) &&
                     CmdReqAdmin() && args.Length == 1)
            {
                try
                {
                    ApplyConditionEffect(new ConditionEffect()
                    {
                        Effect = (ConditionEffectIndex)Enum.Parse(typeof(ConditionEffectIndex), args[0].Trim()),
                        DurationMS = 0
                    });
                    psr.SendPacket(new TextPacket()
                    {
                        BubbleTime = 0,
                        Stars = -1,
                        Name = "",
                        Text = "Success!"
                    });
                }
                catch
                {
                    psr.SendPacket(new TextPacket()
                    {
                        BubbleTime = 0,
                        Stars = -1,
                        Name = "",
                        Text = "Invalid effect!"
                    });
                }
            }
            else if (cmd.Equals("give", StringComparison.OrdinalIgnoreCase) &&
                     CmdReqAdmin() && args.Length >= 1)
            {
                string name = string.Join(" ", args.ToArray()).Trim();
                short objType;
                if (!XmlDatas.IdToType.TryGetValue(name, out objType))
                {
                    psr.SendPacket(new TextPacket()
                    {
                        BubbleTime = 0,
                        Stars = -1,
                        Name = "",
                        Text = "Unknown type!"
                    });
                    return;
                }
                for (int i = 0; i < Inventory.Length; i++)
                    if (Inventory[i] == null)
                    {
                        Inventory[i] = XmlDatas.ItemDescs[objType];
                        UpdateCount++;
                        return;
                        psr.SendPacket(new TextPacket()
                        {
                            BubbleTime = 0,
                            Stars = -1,
                            Name = "",
                            Text = "Success!"
                        });
                    }
            }
            else if (cmd.Equals("tp", StringComparison.OrdinalIgnoreCase) &&
                     CmdReqAdmin() && args.Length >= 2)
            {
                int x, y;
                try
                {
                    x = int.Parse(args[0]);
                    y = int.Parse(args[1]);
                }
                catch
                {
                    psr.SendPacket(new TextPacket()
                    {
                        BubbleTime = 0,
                        Stars = -1,
                        Name = "",
                        Text = "Invalid coordinates!"
                    });
                    return;
                }
                Move(x + 0.5f, y + 0.5f);
                SetNewbiePeriod();
                UpdateCount++;
                Owner.BroadcastPacket(new GotoPacket()
                {
                    ObjectId = Id,
                    Position = new Position()
                    {
                        X = X,
                        Y = Y
                    }
                }, null);
            }
            else if (cmd.Equals("setpiece", StringComparison.OrdinalIgnoreCase) &&
                     CmdReqAdmin() && args.Length == 1)
            {
                try
                {
                    ISetPiece piece = (ISetPiece)Activator.CreateInstance(Type.GetType(
                        "wServer.realm.setpieces." + args[0]));
                    piece.RenderSetPiece(Owner, new IntPoint((int)X + 1, (int)Y + 1));
                    psr.SendPacket(new TextPacket()
                    {
                        BubbleTime = 0,
                        Stars = -1,
                        Name = "",
                        Text = "Success!"
                    });
                }
                catch
                {
                    psr.SendPacket(new TextPacket()
                    {
                        BubbleTime = 0,
                        Stars = -1,
                        Name = "",
                        Text = "Cannot apply setpiece!"
                    });
                }
            }
            else if (cmd.Equals("pause", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    ApplyConditionEffect(new ConditionEffect()
                    {
                        Effect = ConditionEffectIndex.Paused,
                        DurationMS = -1
                    });
                    psr.SendPacket(new TextPacket()
                    {
                        BubbleTime = 0,
                        Stars = -1,
                        Name = "",
                        Text = "Success!"
                    });
                }
                catch
                {
                    psr.SendPacket(new TextPacket()
                    {
                        BubbleTime = 0,
                        Stars = -1,
                        Name = "",
                        Text = "Invalid effect!"
                    });
                }
            }
            else if (cmd.Equals("admin", StringComparison.OrdinalIgnoreCase) &&
                    CmdReqAdmin() && args.Length > 1)
            {
                try
                {
                    Inventory[0] = XmlDatas.ItemDescs[3840];
                    Inventory[1] = XmlDatas.ItemDescs[3843];
                    Inventory[2] = XmlDatas.ItemDescs[3841];
                    Inventory[3] = XmlDatas.ItemDescs[3845];
                    UpdateCount++;
                    return;
                }
                catch
                {
                    psr.SendPacket(new TextPacket()
                    {
                        BubbleTime = 0,
                        Stars = -1,
                        Name = "",
                        Text = "Error!"
                    });
                }
            }
            else if (cmd.Equals("AoR", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    Inventory[3] = XmlDatas.ItemDescs[2878];
                    UpdateCount++;
                    return;
                }
                catch
                {
                    psr.SendPacket(new TextPacket()
                    {
                        BubbleTime = 0,
                        Stars = -1,
                        Name = "",
                        Text = "Error!"
                    });
                }
            }
            else if (cmd.Equals("level", StringComparison.OrdinalIgnoreCase) &&
                     CmdReqAdmin() && args.Length == 1)
            {
                try
                {
                    if (args.Length == 0)
                    {
                        psr.Character.Level = psr.Character.Level + 1;
                        psr.Player.Level = psr.Player.Level + 1;
                        psr.Player.CheckLevelUp();
                        UpdateCount++;
                        psr.SendPacket(new TextPacket()
                        {
                            BubbleTime = 0,
                            Stars = -1,
                            Name = "",
                            Text = "Success!"
                        });
                    }
                    else if (args.Length == 1)
                    {
                        psr.Character.Level = int.Parse(args[0]);
                        psr.Player.Level = int.Parse(args[0]);
                        psr.Player.CheckLevelUp();
                        UpdateCount++;
                        psr.SendPacket(new TextPacket()
                        {
                            BubbleTime = 0,
                            Stars = -1,
                            Name = "",
                            Text = "Success!"
                        });
                    }
                }
                catch
                {
                    psr.SendPacket(new TextPacket()
                    {
                        BubbleTime = 0,
                        Stars = -1,
                        Name = "",
                        Text = "Error!"
                    });
                }
            }
        }
    }
}