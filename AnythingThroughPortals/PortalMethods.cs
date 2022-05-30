using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.ID;
using System;

namespace AnythingThroughPortals
{
	public class PortalMethods
	{
        public static bool anyPortalAtAll = false;
        public static int[,] FoundPortals = new int[256, 2];
        public static int[] PortalCooldownForPlayers = new int[256];
        public static int[] PortalCooldownForNPCs = new int[200];
        public static bool PrevCollide;

        private static void GetPortalEdges(Vector2 position, float angle, out Vector2 start, out Vector2 end)
        {
            //copied straight outta vanilla
            Vector2 value = angle.ToRotationVector2();
            start = position + value * -22f;
            end = position + value * 22f;
        }

        public static void UpdatePortalPoints()
        {
            anyPortalAtAll = false;
            for (int i = 0; i < ((System.Array)FoundPortals).GetLength(0); i++)
            {
                FoundPortals[i, 0] = -1;
                FoundPortals[i, 1] = -1;
            }
            for (int j = 0; j < PortalCooldownForPlayers.Length; j++)
            {
                if (PortalCooldownForPlayers[j] > 0)
                {
                    PortalCooldownForPlayers[j]--;
                }
            }
            for (int k = 0; k < PortalCooldownForNPCs.Length; k++)
            {
                if (PortalCooldownForNPCs[k] > 0)
                {
                    PortalCooldownForNPCs[k]--;
                }
            }
            for (int l = 0; l < 1000; l++)
            {
                Projectile projectile = Main.projectile[l];
                if (projectile.active && projectile.type == 602 && projectile.ai[1] >= 0f && projectile.ai[1] <= 1f && projectile.owner >= 0 && projectile.owner <= 255)
                {
                    FoundPortals[projectile.owner, (int)projectile.ai[1]] = l;
                    if (FoundPortals[projectile.owner, 0] != -1 && FoundPortals[projectile.owner, 1] != -1)
                    {
                        anyPortalAtAll = true;
                    }
                }
            }
        }

        private static Vector2 GetPortalOutingPoint(Vector2 objectSize, Vector2 portalPosition, float portalAngle, out int bonusX, out int bonusY)
        {
            //also copied straight outta vanilla
            int num = (int)Math.Round((double)(MathHelper.WrapAngle(portalAngle) / ((float)Math.PI / 4f)));
            switch (num)
            {
                case -2:
                case 2:
                    bonusX = ((num != 2) ? 1 : (-1));
                    bonusY = 0;
                    return portalPosition + new Vector2((num == 2) ? (0f - objectSize.X) : 0f, (0f - objectSize.Y) / 2f);
                case 0:
                case 4:
                    bonusX = 0;
                    bonusY = ((num == 0) ? 1 : (-1));
                    return portalPosition + new Vector2((0f - objectSize.X) / 2f, (num == 0) ? 0f : (0f - objectSize.Y));
                case -3:
                case 3:
                    bonusX = ((num == -3) ? 1 : (-1));
                    bonusY = -1;
                    return portalPosition + new Vector2((num == -3) ? 0f : (0f - objectSize.X), 0f - objectSize.Y);
                case -1:
                case 1:
                    bonusX = ((num == -1) ? 1 : (-1));
                    bonusY = 1;
                    return portalPosition + new Vector2((num == -1) ? 0f : (0f - objectSize.X), 0f);
                default:
                    bonusX = 0;
                    bonusY = 0;
                    return portalPosition;
            }
        }

        public static void ClonePortalMethod(Entity ent)
        {
            UpdatePortalPoints();

            if (!anyPortalAtAll)
            {
                return;
            }

            float collisionPoint = 0f;
            _ = ent.velocity;
            int width = ent.width;
            int height = ent.height;
            int num = 1;

            for (int i = 0; i < ((System.Array)FoundPortals).GetLength(0); i++)
            {
                if (FoundPortals[i, 0] == -1 || FoundPortals[i, 1] == -1 || (ent is Projectile && (i >= PortalCooldownForNPCs.Length || PortalCooldownForNPCs[i] > 0)) || (ent is Item && (i >= PortalCooldownForNPCs.Length || PortalCooldownForNPCs[i] > 0)))
                {
                    continue;
                }
                for (int j = 0; j < 2; j++)
                {
                    Projectile projectile = Main.projectile[FoundPortals[i, j]];
                    GetPortalEdges(projectile.Center, projectile.ai[0], out var start, out var end);
                    if (!Collision.CheckAABBvLineCollision(ent.position + ent.velocity, ent.Size, start, end, 2f, ref collisionPoint))
                    {
                        continue;
                    }
                    Projectile projectile2 = Main.projectile[FoundPortals[i, 1 - j]];
                    float scaleFactor = ent.Hitbox.Distance(projectile.Center);
                    int bonusX;
                    int bonusY;
                    Vector2 vector = GetPortalOutingPoint(ent.Size, projectile2.Center, projectile2.ai[0], out bonusX, out bonusY) + Vector2.Normalize(new Vector2((float)bonusX, (float)bonusY)) * scaleFactor;
                    Vector2 vector2 = Vector2.UnitX * 16f;
                    if (Collision.TileCollision(vector - vector2, vector2, width, height, fallThrough: true, fall2: true, num) != vector2)
                    {
                        continue;
                    }
                    vector2 = -Vector2.UnitX * 16f;
                    if (Collision.TileCollision(vector - vector2, vector2, width, height, fallThrough: true, fall2: true, num) != vector2)
                    {
                        continue;
                    }
                    vector2 = Vector2.UnitY * 16f;
                    if (Collision.TileCollision(vector - vector2, vector2, width, height, fallThrough: true, fall2: true, num) != vector2)
                    {
                        continue;
                    }
                    vector2 = -Vector2.UnitY * 16f;
                    if (Collision.TileCollision(vector - vector2, vector2, width, height, fallThrough: true, fall2: true, num) != vector2)
                    {
                        continue;
                    }
                    float num2 = 0.1f;
                    if (bonusY == -num)
                    {
                        num2 = 0.1f;
                    }
                    if (ent.velocity == Vector2.Zero)
                    {
                        ent.velocity = (projectile.ai[0] - (float)Math.PI / 2f).ToRotationVector2() * num2;
                    }
                    if (((Vector2)(ent.velocity)).Length() < num2)
                    {
                        ((Vector2)(ent.velocity)).Normalize();
                        ent.velocity *= num2;
                    }
                    Vector2 vector3 = Vector2.Normalize(new Vector2((float)bonusX, (float)bonusY));
                    if (vector3.HasNaNs() || vector3 == Vector2.Zero)
                    {
                        vector3 = Vector2.UnitX * (float)ent.direction;
                    }
                    ent.velocity = vector3 * ((Vector2)(ent.velocity)).Length();
                    if ((bonusY == -num && Math.Sign(ent.velocity.Y) != -num) || Math.Abs(ent.velocity.Y) < 0.1f)
                    {
                        ent.velocity.Y = (float)(-num) * 0.1f;
                    }
                    int num3 = (int)((float)(projectile2.owner * 2) + projectile2.ai[1]);
                    int lastPortalColorIndex = num3 + ((num3 % 2 == 0) ? 1 : (-1));

                    if (ent is Projectile)
                    {
                        if (((Projectile)ent).type != ProjectileID.PortalGunBolt && ((Projectile)ent).type != ProjectileID.PortalGunGate)
                        {
                            PrevCollide = ((Projectile)ent).tileCollide;
                            ((Projectile)ent).tileCollide = false;

                            Projectile proj = (Projectile)ent;
                            proj.position = vector;
                            PortalCooldownForNPCs[i] = 10;
                            if (bonusY == -1 && ent.velocity.Y > -3f)
                            {
                                ent.velocity.Y = -3f;
                            }

                            ((Projectile)ent).tileCollide = PrevCollide;
                        }
                    }
                    if (ent is Item)
                    {
                        Item ite = (Item)ent;
                        ite.position = vector;
                        PortalCooldownForNPCs[i] = 10;
                        if (bonusY == -1 && ent.velocity.Y > -3f)
                        {
                            ent.velocity.Y = -3f;
                        }
                    }
                    return;
                }
            }
        }
    }

    public class PortalProjectileInteraction : GlobalProjectile
    {
        public override void AI(Projectile projectile)
        {
            if (projectile.type != ProjectileID.PortalGunBolt && projectile.type != ProjectileID.PortalGunGate && !projectile.sentry && !ProjectileID.Sets.LightPet[projectile.type] && !ProjectileID.Sets.MinionSacrificable[projectile.type] && !ProjectileID.Sets.IsADD2Turret[projectile.type] && !ProjectileID.Sets.IsAWhip[projectile.type] && !ProjectileID.Sets.MinionTargettingFeature[projectile.type] && !ProjectileID.Sets.StardustDragon[projectile.type] && !projectile.hide && !projectile.minion && !projectile.sentry && !Main.projPet[projectile.type])
                PortalMethods.ClonePortalMethod(projectile);
        }
    }

    public class PortalNPCInteraction : GlobalNPC
    {
        public override void AI(NPC npc)
        {
            if (npc.boss != true && npc.knockBackResist < 100 && npc.dontTakeDamage != true)
            {
                PortalHelper.TryGoingThroughPortals(npc);
            }
        }
    }

    public class PortalItemInteraction : GlobalItem
    {
        public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            PortalMethods.ClonePortalMethod(item);
        }
    }
}