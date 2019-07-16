//Copyright (c) 2019 Jahangmar

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU Lesser General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//GNU Lesser General Public License for more details.

//You should have received a copy of the GNU Lesser General Public License
//along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Objects;
using StardewValley.Projectiles;

namespace PetInteraction
{
    public class ModEntry : Mod
    {
        private enum PetState
        {
            Vanilla,
            CatchingUp,
            Waiting,
            Chasing,
            Fetching,
            Retrieve
        }

        private PetState petState = PetState.Vanilla;

        private Pet pet;

        private Item Stick;

        private Vector2 StickPosition;

        private Stack<Vector2> FetchPath = new Stack<Vector2>();

        private Queue<Vector2> CatchUpPath = new Queue<Vector2>();

        private Config config;

        public override void Entry(IModHelper helper)
        {
            helper.ConsoleCommands.Add("throw", "", (arg1, arg2) =>
            {
                Item wood = ObjectFactory.getItemFromDescription(ObjectFactory.regularObject, Object.wood, 1);
                BasicProjectile proj = new BasicProjectile(0, wood.ParentSheetIndex, 1, 1, 0, 1, 1, Game1.player.Position, null, null, false);
            });

            config = Helper.ReadConfig<Config>();

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.Display.RenderedWorld += Display_RenderedWorld;

        }

        void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            foreach (Vector2 vec in CatchUpPath)
                e.SpriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, vec * 64f), new Rectangle(194 + 0 * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
        }


        void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            pet = null;
            SetState(PetState.Vanilla);
        }


        void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (Game1.currentLocation == null || !Game1.player.hasPet() || GetPet() == null)
                return;

            GameLocation loc = Game1.currentLocation;
            if (e.Button.IsActionButton())
            {
                switch (petState)
                {
                    case PetState.Vanilla:
                        if (GetPet().GetBoundingBox().Contains(e.Cursor.AbsolutePixels) && Helper.Reflection.GetField<bool>(GetPet(), "wasPetToday").GetValue())
                        {
                            Helper.Input.Suppress(e.Button);
                            SetState(PetState.Waiting);
                        }
                        break;
                    case PetState.CatchingUp:
                    case PetState.Waiting:
                    case PetState.Chasing:
                    case PetState.Fetching:
                    case PetState.Retrieve:
                        if (GetPet().GetBoundingBox().Contains(e.Cursor.AbsolutePixels) && Helper.Reflection.GetField<bool>(GetPet(), "wasPetToday").GetValue())
                        {
                            Helper.Input.Suppress(e.Button);
                            SetState(PetState.Vanilla);
                        }
                        break;
                }
            }
            else if (e.Button.IsUseToolButton())
            {

                //TODO throw
                bool thrown = false;
                if (Game1.player.ActiveObject?.ParentSheetIndex == Object.wood)
                {
                    Vector2 velocity = Utility.getVelocityTowardPoint(Game1.player.getTileLocation(), e.Cursor.Tile, 4f);
                    BasicProjectile proj = new BasicProjectile(0, Object.wood, 0, 0, 1, velocity.X, velocity.Y, Game1.player.getTileLocation(), "flameSpellHit", "flameSpell", false, false, Game1.currentLocation, Game1.player);
                    Game1.currentLocation.projectiles.Add(proj);
                    thrown = true;
                }


                switch (petState)
                {
                    case PetState.Vanilla:
                        break;
                    case PetState.CatchingUp:
                    case PetState.Waiting:
                        if (thrown)
                            SetState(PetState.Fetching);
                        break;
                    case PetState.Chasing:
                        break;
                    case PetState.Fetching:
                        break;
                    case PetState.Retrieve:
                        break;
                }
            }
        }

        void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            if (Game1.currentLocation == null || !Game1.player.hasPet() || GetPet() == null)
                return;

            Monitor.Log("Pet velocity: (" + pet.xVelocity + ", " + pet.yVelocity + ")");

            switch (petState)
            {
                case PetState.Vanilla:
                    break;
                case PetState.CatchingUp:
                    if (GetPlayerPetDistance() < 3)
                    {
                        SetState(PetState.Waiting);
                    }
                    break;
                case PetState.Waiting:
                    if (GetPlayerPetDistance() > config.catch_up_distance)
                    {
                        AddCatchUpPosition(new Vector2(pet.getTileX(), pet.getTileY()), new Vector2(Game1.player.getTileX(), Game1.player.getTileY()));
                        //pet.Position = CatchUpPath.Peek() * Game1.tileSize;

                        Monitor.Log("pet.Position: " + pet.Position);
                        Monitor.Log("first square pixels: " + CatchUpPath.Peek() * Game1.tileSize);
                        pet.Position = new Vector2(CatchUpPath.Peek().X, CatchUpPath.Peek().Y) * Game1.tileSize;
                        Monitor.Log("Set pet tile location");
                        Monitor.Log("pet.Position: " + pet.Position);
                        SetState(PetState.CatchingUp);
                    }
                    break;
                case PetState.Chasing:
                    break;
                case PetState.Fetching:
                    //TODO check if pet has reached destination, then pick item up, then retrieve
                    break;
                case PetState.Retrieve:
                    //TODO check if pet has reached player, thenput item down
                    break;
            }
        }

        void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (Game1.currentLocation == null || !Game1.player.hasPet() || GetPet() == null)
                return;

            switch (petState)
            {
                case PetState.Vanilla:
                    break;
                case PetState.CatchingUp:
                    CatchUp();
                    if (GetPetCurrentCatchUpGoalDistance() <= 16)
                    {
                        Monitor.Log("pet.Position is " + pet.Position);
                        Monitor.Log("distance is " + GetPetCurrentCatchUpGoalDistance());
                        Vector2 pos = CatchUpPath.Dequeue();
                        pet.Position = new Vector2(pos.X, pos.Y) * Game1.tileSize;
                        Monitor.Log("distance under threshold, set pet.Position to " + pet.Position);
                        if (CatchUpPath.Count == 0)
                            SetState(PetState.Waiting);

                    }
                    /*
                    else if (GetPetCurrentCatchUpGoalDistance() <= 4 && CatchUpPath.Count <= 3)
                    {
                        SetState(PetState.Waiting);
                    }
                    */
                    break;
                case PetState.Waiting:
                    break;
                case PetState.Chasing:
                    break;
                case PetState.Fetching:
                    break;
                case PetState.Retrieve:
                    break;
            }
        }


        private void SetState(PetState state)
        {
            petState = state;
            Monitor.Log("Set state to " + petState, LogLevel.Trace);
            Game1.showGlobalMessage(petState.ToString());
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

        private Pet FindPet()
        {
            foreach (Character c in Game1.getFarm().characters)
            {
                if (c is Pet p)
                    return p;
            }

            foreach (Character c in Utility.getHomeOfFarmer(Game1.player).characters)
            {
                if (c is Pet p)
                    return p;
            }

            foreach (GameLocation location in Game1.locations)
                foreach (Character c in location.characters)
                {
                    if (c is Pet p)
                        return p;
                }

            return null;
        }

        private Pet GetPet()
        {
            if (pet == null)
                pet = FindPet();
            return pet;
        }

        //in pixels
        public int GetPetCurrentCatchUpGoalDistance() => (int) Utility.distance(GetPet().Position.X, CatchUpPath.Peek().X*Game1.tileSize, GetPet().Position.Y, CatchUpPath.Peek().Y*Game1.tileSize);

        //in tiles
        private int GetPlayerPetDistance() => (int) Utility.distance(GetPet().getTileX(), Game1.player.getTileX(), GetPet().getTileY(), Game1.player.getTileY());

        private Vector2 SnapToTile(Vector2 pos)
        {
            return new Vector2(pos.X - pos.X % Game1.tileSize, pos.Y - pos.Y % Game1.tileSize);
        }

        private void Throw()
        {

        }

        private void SetFetchPosition(Vector2 dest)
        {
            StickPosition = dest;
            //TODO
        }

        private void AddCatchUpPosition(Vector2 src, Vector2 dest)
        {
            Monitor.Log($"Trying to find path from {src} to {dest}");
            Queue<Vector2> path = PathFinder.FindPath(src, dest);
            CatchUpPath = new Queue<Vector2>(path.Take(path.Count-1));
        }

        private void CatchUp()
        {
            GetPet();
            if (pet == null)
                return;

            pet.CurrentBehavior = Pet.behavior_walking;

            Vector2 velocity = Utility.getVelocityTowardPoint(pet.Position, CatchUpPath.Peek()*Game1.tileSize, 2f);
            if (System.Math.Abs(velocity.X) > System.Math.Abs(velocity.Y))
            {
                pet.FacingDirection = velocity.X >= 0 ? 1 : 3;
            }
            else
            {
                pet.FacingDirection = velocity.Y >= 0 ? 2 : 0;
            }
            //pet.Speed
            pet.xVelocity = velocity.X;
            pet.yVelocity = velocity.Y * -1;

            pet.animateInFacingDirection(Game1.currentGameTime);
            pet.setMovingInFacingDirection();
        }

        private void Sit()
        {
            GetPet();
            pet.SetMovingUp(false);
            pet.SetMovingDown(false);
            pet.SetMovingLeft(false);
            pet.SetMovingRight(false);
            pet.CurrentBehavior = Pet.behavior_Sit_Down;
        }


    }
}