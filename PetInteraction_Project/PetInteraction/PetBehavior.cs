// Copyright (c) 2019 Jahangmar
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;

namespace PetInteraction
{
    public class PetBehavior
    {
        public static Pet pet;

        public enum PetState
        {
            Vanilla,
            CatchingUp,
            Waiting,
            Chasing,
            Fetching,
            Retrieve
        }

        private const int pet_max_friendship = 1000; 

        public static PetState petState = PetState.Vanilla;

        public static Stack<Vector2> FetchPath = new Stack<Vector2>();

        public static Queue<Vector2> CatchUpPath = new Queue<Vector2>();

        private static Item Stick;

        private static Vector2 StickPosition;

        public static void SetState(PetState state)
        {
            if (petState == state)
                return;

            petState = state;
            ModEntry.Log("Set state to " + petState);

            switch (state)
            {
                case PetState.Vanilla:
                    break;
                case PetState.CatchingUp:
                    break;
                case PetState.Waiting:
                    Sit();
                    CatchUpPath.Clear();
                    break;
                case PetState.Chasing:
                    break;
                case PetState.Fetching:
                    break;
                case PetState.Retrieve:
                    break;
            }
        }

        private static Pet FindPet()
        {
            bool check(Character c) => c is Pet p && p != ModEntry.TempPet;

            foreach (Character c in Game1.getFarm().characters)
            {
                if (check(c))
                    return c as Pet;
            }

            foreach (Character c in Utility.getHomeOfFarmer(Game1.player).characters)
            {
                if (check(c))
                    return c as Pet;
            }

            foreach (GameLocation location in Game1.locations)
                foreach (Character c in location.characters)
                {
                    if (check(c))
                        return c as Pet;
                }

            return null;
        }

        public static Pet GetPet()
        {
            if (pet == null)
                pet = FindPet();
            return pet;
        }

        /// <summary>
        /// Returns distance (in pixels) from pet position to current goal of the path. 
        /// </summary>
        public static int PetCurrentCatchUpGoalDistance() => CatchUpPath.Count == 0 ? 0 : (int)Utility.distance(GetPet().Position.X, CatchUpPath.Peek().X * Game1.tileSize, GetPet().Position.Y, CatchUpPath.Peek().Y * Game1.tileSize);

        /// <summary>
        /// Returns distance between pet and player in tiles.
        /// </summary>
        public static int PlayerPetDistance() => (int)Utility.distance(GetPet().getTileX(), Game1.player.getTileX(), GetPet().getTileY(), Game1.player.getTileY());

        public static int PetDistance(Vector2 tile) => (int)Utility.distance(GetPet().getTileX(), tile.X, GetPet().getTileY(), tile.Y);

        private static int Distance(Vector2 vec1, Vector2 vec2) => (int)Utility.distance(vec1.X, vec2.X, vec1.Y, vec2.Y);

        public static void SetPetPositionFromTile(Vector2 tile) => GetPet().Position = tile * Game1.tileSize;

        public static void Throw()
        {

        }

        private static void SetFetchPosition(Vector2 dest)
        {
            StickPosition = dest;
            //TODO
        }

        /// <summary>
        /// Returns velocity of pet towards current goal of the path.
        /// </summary>
        private static Vector2 GetVelocity()
        {
            if (CatchUpPath.Count == 0)
                return new Vector2(0, 0);

            Vector2 pathPosition = CatchUpPath.Peek() * Game1.tileSize;
            if ((int)pathPosition.X == (int)pet.Position.X && (int)pathPosition.Y == (int)pet.Position.Y)
            {
                return new Vector2(0, 0);
            }
            else
            {
                int speed = ModEntry.config.pet_speed;
                switch (petState)
                {
                    case PetState.CatchingUp:
                        speed = ModEntry.config.pet_speed;
                        break;
                    case PetState.Chasing:
                        speed = 10;
                        break;
                }
                return Utility.getVelocityTowardPoint(pet.Position, pathPosition, speed);
            }
        }

        public static void CatchUp()
        {
            GetPet();
            if (pet == null)
                return;

            SetPetBehavior(Pet.behavior_walking);

            Vector2 velocity = GetVelocity();

            if (System.Math.Abs(velocity.X) > System.Math.Abs(velocity.Y))
                pet.FacingDirection = velocity.X >= 0 ? 1 : 3;
            else
                pet.FacingDirection = velocity.Y >= 0 ? 2 : 0;

            pet.xVelocity = velocity.X;
            pet.yVelocity = -velocity.Y;

            pet.animateInFacingDirection(Game1.currentGameTime);
            pet.setMovingInFacingDirection();
        }

        public static void Sit()
        {
            GetPet();
            pet.SetMovingUp(false);
            pet.SetMovingDown(false);
            pet.SetMovingLeft(false);
            pet.SetMovingRight(false);
            SetPetBehavior(Pet.behavior_Sit_Down);
        }

        private static void SetPetBehavior(int behavior)
        {
            ModEntry.PetBehaviour = behavior;
        }

        public static void Jump()
        {
            GetPet().jump();
        }

        public static void GetHitByTool()
        {
            SetState(PetState.Vanilla);
            GetPet().doEmote(Character.angryEmote);
            Jump();
            pet.friendshipTowardFarmer = System.Math.Max(0, pet.friendshipTowardFarmer - ModEntry.config.pet_friendship_decrease_onhit ); // Values from StardewValley.Characters.Pet
            ModEntry.GetHelper().Reflection.GetField<bool>(GetPet(), "wasPetToday").SetValue(false);
        }

        public static void TryChaseCritterInRange()
        {
            foreach (Critter critter in ModEntry.GetHelper().Reflection.GetField<List<Critter>>(Game1.currentLocation, "critters").GetValue())
            {
                if ((critter is Birdie || critter is Seagull || critter is Rabbit || critter is Squirrel) && PetDistance(critter.position / Game1.tileSize) < 20)
                {
                    Queue<Vector2> path = PathFinder.CalculatePath(GetPet(), critter.position / Game1.tileSize);
                    if (path.Count > 0)
                    {
                        CatchUpPath = path;
                        SetState(PetState.Chasing);

                    }
                    else
                    {
                        var random = new System.Random();
                        if (random.Next(20) == 0)
                        {
                            GetPet().doEmote(Character.questionMarkEmote);
                            GetPet().playContentSound();
                        }
                    }

                }
            }
        }

        public static void CannotReachPlayer()
        {
            var random = new System.Random();
            if (random.Next(10) == 0)
            {
                GetPet().doEmote(Character.questionMarkEmote);
                pet.playContentSound();
            }
        }
    }
}
