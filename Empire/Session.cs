using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire
{
    public class Session
    {
        public readonly EmpireGame Game;
        public Planet Planet;
        public int Day = 0;

        public Session(EmpireGame game)
        {
            Game = game;
            Planet = new Planet(this);
        }

        public void Update()
        {
            Day++;
            Planet.Update();
        }
    }
}
