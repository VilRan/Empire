using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire
{
    public class PlanetView
    {
        Planet planet;
        KeyboardState previousKeyboard;
        MouseState previousMouse;
        Matrix world;
        Matrix view;
        Matrix projection;
        Vector3 cameraPosition;
        Vector3 viewDirection;
        float viewTargetDistance = 3f;
        float viewDistance = 3f;
        float viewLatitude = 0f;
        float viewLongitude = 0f;

        EmpireGame game { get { return planet.Game; } }
        Session session { get { return planet.Session; } }
        Random random { get { return game.Random; } }
        GraphicsDevice graphicsDevice { get { return game.GraphicsDevice; } }
        Effect effect { get { return game.PlanetShader; } }
        int width { get { return game.Width; } }
        int height { get { return game.Height; } }

        public PlanetView(Planet planet)
        {
            this.planet = planet;
            world = Matrix.CreateTranslation(0, 0, 0);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), (float)width / height, 0.01f, 100f);
        }

        public void Update()
        {
            KeyboardState keyboard = Keyboard.GetState();
            //TODO
            previousKeyboard = keyboard;

            MouseState mouse = Mouse.GetState();

            int scrollDelta = mouse.ScrollWheelValue - previousMouse.ScrollWheelValue;
            if (scrollDelta < 0)
                viewTargetDistance *= (float)Math.Pow(1.0005, scrollDelta);
            else if (scrollDelta > 0)
                viewTargetDistance /= (float)Math.Pow(1.0005, -scrollDelta);
            if (viewTargetDistance < 1.2f)
                viewTargetDistance = 1.2f;
            viewDistance = (viewDistance + viewTargetDistance) / 2;

            if (mouse.RightButton == ButtonState.Pressed)
            {
                Point positionDelta = mouse.Position - previousMouse.Position;
                viewLongitude -= -0.00125f * positionDelta.X * (viewDistance - 1);
                viewLatitude -= -0.00125f * positionDelta.Y * (viewDistance - 1);
                if (viewLatitude > MathHelper.PiOver2)
                    viewLatitude = MathHelper.PiOver2;
                if (viewLatitude < -MathHelper.PiOver2)
                    viewLatitude = -MathHelper.PiOver2;
            }

            cameraPosition = viewDistance * new Vector3(
                (float)(Math.Cos(viewLongitude) * Math.Cos(viewLatitude)),
                (float)Math.Sin(viewLatitude),
                (float)(Math.Sin(viewLongitude) * Math.Cos(viewLatitude)));
            view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
            viewDirection = Vector3.Normalize(-cameraPosition);

            previousMouse = mouse;
        }

        public void Draw()
        {
            Matrix worldViewProjection = world * view * projection;
            Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(world));
            effect.Parameters["World"].SetValue(world);
            effect.Parameters["WorldViewProjection"].SetValue(worldViewProjection);
            effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);
            effect.Parameters["AmbientIntensity"].SetValue(0.01f);
            effect.Parameters["DiffuseColor"].SetValue(Color.White.ToVector4());
            effect.Parameters["LightDirection"].SetValue(-viewDirection);
            effect.Parameters["Shininess"].SetValue(1f);
            effect.Parameters["SpecularColor"].SetValue(new Vector4(0.1f, 0.1f, 0.1f, 1f));
            effect.Parameters["ViewDirection"].SetValue(viewDirection);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                planet.Draw();
            }
        }
    }
}
