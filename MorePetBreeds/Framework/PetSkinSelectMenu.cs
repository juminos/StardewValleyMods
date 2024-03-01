using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;

namespace MorePetBreeds.Framework
{
    class PetSkinSelectMenu : IClickableMenu
    {
        // Handling Skin
        public int currentSkinId = 1;
        public Dictionary<string, Texture2D> skinTextureMap;

        // Menu Textures
        public ClickableTextureComponent petPreview;
        public ClickableTextureComponent backButton;
        public ClickableTextureComponent forwardButton;
        public ClickableTextureComponent okButton;

        // Constants
        private static readonly int petSpriteWidth, petSpriteHeight = petSpriteWidth = 32;
        private static readonly float petPreviewScale = 4f;
        private static readonly int petSpriteIndex = 4;
        private static readonly int menuPadding = 64;
        private static readonly int okButtonWidth, okButtonHeight = okButtonWidth = 64;
        private static readonly int backButtonWidth, forwardButtonWidth = backButtonWidth = 48;
        private static readonly int backButtonHeight, forwardButtonHeight = backButtonHeight = 44;

        private static readonly int maxWidthOfMenu = petSpriteWidth * (int)petPreviewScale + menuPadding;
        private static readonly int maxHeightOfMenu = petSpriteHeight * (int)petPreviewScale + menuPadding;

        private readonly int backButtonId = 44;
        private readonly int forwardButtonId = 33;
        private readonly int okButtonId = 46;

        public PetSkinSelectMenu(Dictionary<string, Texture2D> skinTextureMap)
        {
            this.skinTextureMap = new Dictionary<string, Texture2D>(skinTextureMap);
            resetBounds();
        }
        public Texture2D CurrentPetTexture => skinTextureMap[currentSkinId.ToString()];

        public override void receiveGamePadButton(Buttons b)
        {
            // TODO: add fix for controller
            base.receiveGamePadButton(b);
            if (b == Buttons.LeftTrigger)
            {
                currentSkinId--;
                if (currentSkinId < 1)
                    currentSkinId = skinTextureMap.Count;

                Game1.playSound("shwip");
                backButton.scale = backButton.baseScale;
                updatePetPreview();
            }
            if (b == Buttons.RightTrigger)
            {
                currentSkinId++;
                if (currentSkinId > skinTextureMap.Count)
                    currentSkinId = 1;

                forwardButton.scale = forwardButton.baseScale;
                Game1.playSound("shwip");
                updatePetPreview();
            }
            if (b == Buttons.A)
            {
                selectSkin();
                exitThisMenu();
                Game1.playSound("smallSelect");
            }
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            if (backButton.containsPoint(x, y))
            {
                currentSkinId--;
                if (currentSkinId < 1)
                    currentSkinId = skinTextureMap.Count;

                Game1.playSound("shwip");
                backButton.scale = backButton.baseScale;
                updatePetPreview();
            }
            if (forwardButton.containsPoint(x, y))
            {
                currentSkinId++;
                if (currentSkinId > skinTextureMap.Count)
                    currentSkinId = 1;

                forwardButton.scale = forwardButton.baseScale;
                Game1.playSound("shwip");
                updatePetPreview();
            }
            if (okButton.containsPoint(x, y))
            {
                selectSkin();
                exitThisMenu();
                Game1.playSound("smallSelect");
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            backButton.tryHover(x, y);
            forwardButton.tryHover(x, y);
            okButton.tryHover(x, y);
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            resetBounds();
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.5f);

            drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);
            base.draw(b);
            petPreview.draw(b);
            backButton.draw(b);
            forwardButton.draw(b);
            okButton.draw(b);
            drawMouse(b);
        }

        private void selectSkin()
        {
            ModEntry.SetPetSkin(currentSkinId);
            Game1.activeClickableMenu = new NamingMenu(ModEntry.AddPet, $"What will you name it?");

        }



        private void updatePetPreview()
        {
            petPreview = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + menuPadding, yPositionOnScreen + menuPadding, petSpriteWidth, petSpriteHeight), CurrentPetTexture, Game1.getSourceRectForStandardTileSheet(CurrentPetTexture, petSpriteIndex, petSpriteWidth, petSpriteHeight), petPreviewScale);
        }
        private void resetBounds()
        {
            xPositionOnScreen = Game1.uiViewport.Width / 2 - maxWidthOfMenu / 2 - spaceToClearSideBorder;
            yPositionOnScreen = Game1.uiViewport.Height / 2 - maxHeightOfMenu / 2 - spaceToClearTopBorder;
            width = maxWidthOfMenu + spaceToClearSideBorder;
            height = maxHeightOfMenu + spaceToClearTopBorder;
            initialize(xPositionOnScreen, yPositionOnScreen, width + menuPadding, height + menuPadding, showUpperRightCloseButton: true);

            updatePetPreview();

            backButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + menuPadding, yPositionOnScreen + petSpriteHeight * (int)petPreviewScale + menuPadding, backButtonWidth, backButtonHeight), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, backButtonId), 1f)
            {
                myID = backButtonId,
                rightNeighborID = forwardButtonId
            };
            forwardButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - menuPadding - forwardButtonWidth, yPositionOnScreen + petSpriteHeight * (int)petPreviewScale + menuPadding, forwardButtonWidth, forwardButtonHeight), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, forwardButtonId), 1f)
            {
                myID = forwardButtonId,
                leftNeighborID = backButtonId,
                rightNeighborID = okButtonId
            };
            okButton = new ClickableTextureComponent("OK", new Rectangle(xPositionOnScreen + width - okButtonWidth - menuPadding / 4, yPositionOnScreen + height - okButtonHeight - menuPadding / 4, okButtonWidth, okButtonHeight), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, okButtonId), 1f)
            {
                myID = okButtonId,
                leftNeighborID = forwardButtonId,
                rightNeighborID = -99998
            };
        }
    }
}
