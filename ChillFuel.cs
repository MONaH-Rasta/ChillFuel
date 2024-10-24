using Oxide.Game.Rust.Cui;
using UnityEngine;
using System;
using Rust;
using System.Collections.Generic;
using Oxide.Core;
using UnityEngine.UI;
using Network;

namespace Oxide.Plugins
{
    [Info("Chill fuel", "Thisha", "0.1.2")]
    public class ChillFuel : RustPlugin
    {
        private const string inviteNoticeMsg = "assets/bundled/prefabs/fx/invite_notice.prefab";
        private const string minicopterShortName = "minicopter.entity";
        private const string transportheliShortName = "scraptransporthelicopter";
        private const string rowboatShortName = "rowboat";
        private const string RHIBShortName = "rhib";
        private const string fuelpermissionName = "chillfuel.use";

        private Dictionary<ulong, bool> playerData = new Dictionary<ulong, bool>();
        
        #region localization
        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["HelpInfo"] = "Use \"/fuel on\" to show the fuel amount.\nUse \"/fuel off\" to turn it off."
            }, this);
        }
        #endregion localization

        #region commands
        [ChatCommand("Fuel")]
        void HandleChatcommand(BasePlayer player, string command, string[] args)
        {
            if (args.Length == 1)
            {
                switch (args[0])
                {
                    case "on":
                        UpdateState(player,true);
                        break;

                    case "off":
                        UpdateState(player,false);
                        break;

                    default:
                        ShowCommandHelp(player);
                        break;
                }
            }
            else
                ShowCommandHelp(player);
        }

        void ShowCommandHelp(BasePlayer player)
        {
            player.ChatMessage(Lang("HelpInfo",player.UserIDString));
        }
        #endregion commands

        #region Hooks
        private void Init()
        {
            permission.RegisterPermission(fuelpermissionName, this);

            LoadData();
        }

        private void Unload()
        {
            foreach (BasePlayer player in BasePlayer.activePlayerList)
            {
                if (!player.IsAlive())
                    continue;

                if (!permission.UserHasPermission(player.UserIDString, fuelpermissionName))
                    continue;
                
                if (!PlayerSignedUp(player))
                    continue;

                DestroyUI(player, true);
            }
        }

        void OnPlayerDeath(BasePlayer player, ref HitInfo info)
        {
            if (!permission.UserHasPermission(player.UserIDString, fuelpermissionName))
                return;

            if (!PlayerSignedUp(player))
                return;

            DestroyUI(player, true);
        }

        void OnEntityMounted(BaseMountable entity, BasePlayer player)
        {
            if (!permission.UserHasPermission(player.UserIDString, fuelpermissionName))
                return;
            
            if (!PlayerSignedUp(player))
                return;

            BaseEntity be = entity.GetParentEntity();
            if (be == null)
                return;

            if (be.ShortPrefabName.Equals(minicopterShortName))
            {
                MiniCopter copter = be.GetComponentInChildren<MiniCopter>();
                if (copter != null) 
                {
                    if (copter.GetFuelSystem().fuelStorageInstance.IsValid(true))
                    {
                        UpdatePanels(player, copter.GetFuelSystem().GetFuelAmount(), true);
                        DoPlayerTime(player, false);
                    }
                }
            } 
            else
            {
                if (be.ShortPrefabName.Equals(transportheliShortName))
                {
                    MiniCopter copter = be.GetComponentInParent<MiniCopter>();
                    if (copter != null)
                    {
                        if (copter.GetFuelSystem().fuelStorageInstance.IsValid(true))
                        {
                            UpdatePanels(player, copter.GetFuelSystem().GetFuelAmount(), true);
                            DoPlayerTime(player, false);
                        }
                    }
                } 
                else
                {
                    if (be.ShortPrefabName.Equals(rowboatShortName))
                    {
                        MotorRowboat boat = be.GetComponentInChildren<MotorRowboat>();
                        if (boat != null)
                        {
                            UpdatePanels(player, boat.fuelSystem.GetFuelAmount(), true);
                            DoPlayerTime(player, false);
                        }
                    } 
                    else
                    {
                        if (be.ShortPrefabName.Equals(RHIBShortName))
                        {
                            RHIB rhib = be.GetComponentInChildren<RHIB>();
                            if (rhib != null)
                            {
                                UpdatePanels(player, rhib.fuelSystem.GetFuelAmount(), true);
                                DoPlayerTime(player, false);
                            }
                        }
                        else
                        {
                            ModularCar car = be.GetComponentInChildren<ModularCar>();
                            if (car != null)
                            {
                                UpdatePanels(player, car.fuelSystem.GetFuelAmount(), true);
                                DoPlayerTime(player, false);
                            }
                        }
                    }
                }
            }
        }

        void OnEntityDismounted(BaseMountable entity, BasePlayer player)
        {
            if (!permission.UserHasPermission(player.UserIDString, fuelpermissionName))
                return;

            if (!PlayerSignedUp(player))
                return;

            DestroyUI(player, true);
        }
        #endregion Hooks

        #region Functions
        void DoPlayerTime(BasePlayer player, bool updatePicture)
        {
            if (player == null)
                return;
            
            if (!PlayerSignedUp(player))
            {
                DestroyUI(player, true);
                return;
            }

            if (player.isMounted)
            {
                timer.Once(5f, () =>
                {
                    CheckAction(player, updatePicture);
                });
            } 
            else
            {
                DestroyUI(player,true);
            }
        }

        void CheckAction(BasePlayer player, bool updatePicture)
        {
            BaseVehicle veh = player.GetMountedVehicle();
            if (veh != null)
            {
                MiniCopter copter = veh.GetComponentInChildren<MiniCopter>();
                if (copter == null)
                    copter = veh.GetComponentInParent<MiniCopter>();

                if (copter != null)
                {
                    if (copter.GetFuelSystem().fuelStorageInstance.IsValid(true))
                    {
                        UpdatePanels(player, copter.GetFuelSystem().GetFuelAmount(), updatePicture);
                        DoPlayerTime(player, false);
                    }
                }
                else
                {
                    RHIB rhib = veh.GetComponentInParent<RHIB>();
                    if (rhib != null)
                    {
                        UpdatePanels(player, rhib.fuelSystem.GetFuelAmount(), updatePicture);
                        DoPlayerTime(player, false);
                    }
                    else
                    {
                        MotorRowboat motorBoat = veh.GetComponentInParent<MotorRowboat>();
                        if (motorBoat != null)
                        {
                            UpdatePanels(player, motorBoat.fuelSystem.GetFuelAmount(), updatePicture);
                            DoPlayerTime(player, false);
                        }
                        else
                        {
                            ModularCar car = veh.GetComponentInParent<ModularCar>();
                            if (car != null)
                            {
                                UpdatePanels(player, car.fuelSystem.GetFuelAmount(), updatePicture);
                                DoPlayerTime(player, false);
                            }
                            else
                                DestroyUI(player, true);
                        }
                    }
                }
            }
            else
            {
                DestroyUI(player, true);
            }
        }

        void UpdateState(BasePlayer player, bool newState)
        {
            bool doSave = false;

            if (!playerData.ContainsKey(player.userID))
            {
                playerData.Add(player.userID, newState);
                doSave = true;
            }
            else
            {
                if (playerData[player.userID] != newState)
                {
                    playerData[player.userID] = newState;
                    doSave = true;
                }
            }

            if (doSave)
            {
                SaveData();
                DestroyUI(player,true);
                if (newState == true)
                    CheckAction(player, true);
            }
        }

        void LoadData()
        {
            try
            {
                playerData = Interface.GetMod().DataFileSystem.ReadObject<Dictionary<ulong, bool>>("ChillFuel");
            }
            catch
            {
                playerData = new Dictionary<ulong, bool>();
            }
        }
        
        void SaveData() => Interface.Oxide.DataFileSystem.WriteObject("ChillFuel", playerData);
        #endregion Functions    

        #region ui
        void UpdatePanels(BasePlayer player, float condition, bool doPicture)
        {
            string color = "1 1 1 255";
            string valueText;

            if (condition < 0)
                condition = 0;

            valueText = ((int)Math.Round(condition, 0)).ToString();

            DestroyUI(player, doPicture);
            DrawUI(player, color, valueText, doPicture);
        }

        void DestroyUI(BasePlayer player, bool updatePicture)
        {
            CuiHelper.DestroyUi(player, "fuelmeterpanel");
            if (updatePicture)
                CuiHelper.DestroyUi(player, "fuelmeterpicture");
        }

        void DrawUI(BasePlayer player, string color, string valueText, bool updatePicture)
        {
            CuiElementContainer menu = Generate_Menu(player, color, valueText, updatePicture);
            CuiHelper.AddUi(player, menu);
        }

        CuiElementContainer Generate_Menu(BasePlayer player, string color, string valueText, bool updatePicture)
        {
            var elements = new CuiElementContainer();
            var info01 = elements.Add(new CuiLabel
            {
                Text =
                {
                    Text = valueText,
                    Color = "1 1 1 255",
                    FontSize = 13,
                    Align = TextAnchor.MiddleLeft
                },

                RectTransform = {
                    AnchorMin = "0.300 0.010",    
                    AnchorMax = "0.315 0.030"     
                },
            }, "Hud", "fuelmeterpanel");

            if (updatePicture)
            {
                var elements2 = new CuiElementContainer();
                elements2.Add(new CuiElement
                {
                    Name = "fuelmeterpicture",
                    Components =
                    {
                        new CuiRawImageComponent
                        {
                            Color = "1 1 1 1",
                            Url = "http://76lin1nucleusbe.webhosting.be/rust/fuel.png"
                        },
                        new CuiRectTransformComponent
                        {
                            AnchorMin = "0.285 0.010",    
                            AnchorMax = "0.295 0.030"     
                        }
                    }
                });

                CuiHelper.AddUi(player, elements2);
            }

            return elements;
        }
        #endregion ui
        
        #region helpers
        private bool PlayerSignedUp(BasePlayer player)
        {
            if (playerData.ContainsKey(player.userID))
                return playerData[player.userID];
            else
                return true;
        }

        private string Lang(string key, string userId = null, params object[] args) => string.Format(lang.GetMessage(key, this, userId), args);
        #endregion helpers
    }
}