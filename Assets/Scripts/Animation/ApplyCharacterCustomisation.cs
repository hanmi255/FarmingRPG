using System.Collections.Generic;
using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.Animation
{
    [System.Serializable]
    public class ColorSwap
    {
        public Color originalColor;
        public Color newColor;

        public ColorSwap(Color originalColor, Color newColor)
        {
            this.originalColor = originalColor;
            this.newColor = newColor;
        }
    }

    /// <summary>
    /// 衬衫像素数据结构，包含不同朝向的像素数组
    /// </summary>
    [System.Serializable]
    public struct ShirtPixelData
    {
        public Color[] FrontPixels;
        public Color[] BackPixels;
        public Color[] RightPixels;
    }

    /// <summary>
    /// 身体装饰像素数据结构，包含不同朝向的像素数组
    /// </summary>
    [System.Serializable]
    public struct AdornmentsPixelData
    {
        public Color[] FrontPixels;
        public Color[] RightPixels;
    }

    /// <summary>
    /// 身体部分处理类型枚举
    /// </summary>
    public enum BodyPartProcessType
    {
        Shirt,
        Adornments
    }

    public class ApplyCharacterCustomisation : MonoBehaviour
    {
        #region Fields
        [Header("Base Textures")]
        [SerializeField] private Texture2D _maleFarmerBaseTexture = null;
        [SerializeField] private Texture2D _femaleFarmerBaseTexture = null;
        [SerializeField] private Texture2D _shirtBaseTexture = null;
        [SerializeField] private Texture2D _hariBaseTexture = null;
        [SerializeField] private Texture2D _hatBaseTexture = null;
        [SerializeField] private Texture2D _adornmentsBaseTexture = null;
        private Texture2D _farmerBaseTexture;

        [Header("Output Base Texture To Be Used For Animation")]
        [SerializeField] private Texture2D _farmerBaseCustomised = null;
        [SerializeField] private Texture2D _hairCustomised = null;
        [SerializeField] private Texture2D _hatCustomised = null;

        private Texture2D _farmerBaseShirtUpdated;
        private Texture2D _farmerBaseAdornmentsUpdated;
        private Texture2D _selectedShirt;
        private Texture2D _selectedAdornments;

        [Header("Select Shirt Style")]
        [Range(0, 1)]
        [SerializeField] private int _shirtStyle = 0;

        [Header("Select Hair Style")]
        [Range(0, 2)]
        [SerializeField] private int _hairStyle = 0;

        [Header("Select Hat Style")]
        [Range(0, 1)]
        [SerializeField] private int _hatStyle = 0;

        [Header("Select Adornments Style")]
        [Range(0, 2)]
        [SerializeField] private int _adornmentsStyle = 0;

        [Header("Select Skin Type")]
        [Range(0, 3)]
        [SerializeField] private int _skinType = 0;

        [Header("Select Sex:0-Male, 1-Female")]
        [Range(0, 1)]
        [SerializeField] private int _sex = 0;

        [Header("Select Hair Color")]
        [SerializeField] private Color _hairrColor = Color.black;

        [Header("Select Trouser Color")]
        [SerializeField] private Color _trouserColor = Color.blue;

        // Arrays
        private Facing[,] _bodyFacingArray;
        private Vector2Int[,] _bodyShirtOffsetArray;
        private Vector2Int[,] _bodyAdornmentsOffsetArray;

        // Dimensions
        private readonly int _bodyRows = 21;
        private readonly int _bodyColumns = 6;

        // Constants for optimization
        private static readonly Vector2Int INVALID_OFFSET = new(99, 99);

        private readonly int _farmerSpriteWidth = 16;
        private readonly int _farmerSpriteHeight = 32;

        private readonly int _shirtTextureWidth = 9;
        private readonly int _shirtTextureHeight = 36;
        private readonly int _shirtSpriteWidth = 9;
        private readonly int _shirtSpriteHeight = 9;
        private readonly int _shirtStyleInSpriteWidth = 16;

        private readonly int _adornmentsTextureWidth = 16;
        private readonly int _adornmentsTextureHeight = 32;
        private readonly int _adornmentsSpriteWidth = 16;
        private readonly int _adornmentsSpriteHeight = 16;
        private readonly int _adornmentsStyleInSpriteWidth = 8;

        private readonly int _hairTextureWidth = 16;
        private readonly int _hairTextureHeight = 96;
        private readonly int _hairStyleInSpriteWidth = 8;

        private readonly int _hatTextureWidth = 20;
        private readonly int _hatTextureHeight = 80;
        private readonly int _hatStyleInSpriteWidth = 12;

        // Color Swapping
        private List<ColorSwap> _colorSwapList;

        // For Arms
        private Color32 _armTargetColor1 = new(77, 13, 13, 255);  // Dark Red
        private Color32 _armTargetColor2 = new(138, 41, 41, 255); // Light Red
        private Color32 _armTargetColor3 = new(172, 50, 50, 255); // Lighter Red

        // For Skin
        private Color32 _skinTargetColor1 = new(145, 117, 90, 255); // Dark Brown
        private Color32 _skinTargetColor2 = new(204, 155, 108, 255); // Light Brown
        private Color32 _skinTargetColor3 = new(207, 166, 128, 255); // Lighter Brown
        private Color32 _skinTargetColor4 = new(238, 195, 154, 255); // Lightest Brown

        /// <summary>
        /// 皮肤颜色数据定义 - 索引对应skinType (0-3)
        /// 每个数组包含4个颜色，对应_skinTargetColor1-4的替换颜色
        /// </summary>
        private static readonly Color32[][] _skinColorData = {
            // SkinType 0 - 默认皮肤（保持原始颜色）
            new Color32[] { new(145, 117, 90, 255), new(204, 155, 108, 255), new(207, 166, 128, 255), new(238, 195, 154, 255) },
            // SkinType 1 - 浅色皮肤
            new Color32[] { new(187, 157, 128, 255), new(231, 187, 144, 255), new(221, 186, 154, 255), new(213, 189, 167, 255) },
            // SkinType 2 - 深色皮肤
            new Color32[] { new(105, 69, 2, 255), new(128, 87, 12, 255), new(145, 103, 26, 255), new(161, 114, 25, 255) },
            // SkinType 3 - 特殊皮肤
            new Color32[] { new(151, 132, 0, 255), new(187, 166, 15, 255), new(209, 188, 39, 255), new(211, 199, 112, 255) }
        };

        /// <summary>
        /// 目标皮肤颜色数组 - 对应需要替换的原始颜色
        /// </summary>
        private Color32[] TargetSkinColors => new[] { _skinTargetColor1, _skinTargetColor2, _skinTargetColor3, _skinTargetColor4 };
        #endregion

        #region Lifecycle Methods
        private void Awake()
        {
            _colorSwapList = new List<ColorSwap>();

            ProcessCustomisation();
        }
        #endregion

        #region Private Methods
        private void ProcessCustomisation()
        {
            ProcessGender();
            ProcessShirt();
            ProcessArms();
            ProcessTrousers();
            ProcessHair();
            ProcessSkin();
            ProcessHat();
            ProcessAdornments();

            MergeCustomisation();
        }

        private void ProcessGender()
        {
            _farmerBaseTexture = _sex == 0 ? _maleFarmerBaseTexture : _femaleFarmerBaseTexture;

            var farmerBasePixels = _farmerBaseTexture.GetPixels();
            _farmerBaseCustomised.SetPixels(farmerBasePixels);
            _farmerBaseCustomised.Apply();
        }

        private void ProcessShirt()
        {
            _bodyFacingArray = new Facing[_bodyColumns, _bodyRows];
            PopulateBodyFacingArray();

            _bodyShirtOffsetArray = new Vector2Int[_bodyColumns, _bodyRows];
            PopulateBodyShirtOffsetArray();

            AddShirtToTexture(_shirtStyle);

            ApplyShirtTextureToBase();
        }

        private void ProcessArms()
        {
            var farmerPixelsToRecolor = _farmerBaseTexture.GetPixels(0, 0, 288, _farmerBaseTexture.height);

            PopulateArmColorSwapList();

            ChangePixelsColor(farmerPixelsToRecolor, _colorSwapList);

            _farmerBaseCustomised.SetPixels(0, 0, 288, _farmerBaseTexture.height, farmerPixelsToRecolor);
            _farmerBaseCustomised.Apply();
        }

        private void ProcessTrousers()
        {
            var farmerTrouserPixels = _farmerBaseTexture.GetPixels(288, 0, 96, _farmerBaseTexture.height);

            TintPixelColors(farmerTrouserPixels, _trouserColor);

            _farmerBaseCustomised.SetPixels(288, 0, 96, _farmerBaseTexture.height, farmerTrouserPixels);
            _farmerBaseCustomised.Apply();
        }

        private void ProcessHair()
        {
            AddHairToTexture(_hairStyle);

            var farmerSelectedHairPixels = _hairCustomised.GetPixels();

            TintPixelColors(farmerSelectedHairPixels, _hairrColor);

            _hairCustomised.SetPixels(farmerSelectedHairPixels);
            _hairCustomised.Apply();
        }

        private void ProcessSkin()
        {
            var farmerPixelsToRecolor = _farmerBaseCustomised.GetPixels(0, 0, 288, _farmerBaseTexture.height);

            PopulateSkinColorSwapList(_skinType);

            ChangePixelsColor(farmerPixelsToRecolor, _colorSwapList);

            _farmerBaseCustomised.SetPixels(0, 0, 288, _farmerBaseTexture.height, farmerPixelsToRecolor);
            _farmerBaseCustomised.Apply();
        }

        private void ProcessHat()
        {
            AddHatToTexture(_hatStyle);
        }

        private void ProcessAdornments()
        {
            _bodyAdornmentsOffsetArray = new Vector2Int[_bodyColumns, _bodyRows];
            PopulateBodyAdornmentsOffsetArray();

            AddAdornmentsToTexture(_adornmentsStyle);

            ApplyAdornmentsTextureToBase();
        }

        private void MergeCustomisation()
        {
            var blockWidth = _bodyColumns * _farmerSpriteWidth;
            var blockHeight = _farmerBaseTexture.height;

            var farmerShirtPixels = _farmerBaseShirtUpdated.GetPixels(0, 0, blockWidth, blockHeight);
            var farmerAdornmentsPixels = _farmerBaseAdornmentsUpdated.GetPixels(0, 0, blockWidth, blockHeight);
            var farmerTrousePixelsSelection = _farmerBaseCustomised.GetPixels(288, 0, 96, blockHeight);
            var farmerBodyPixels = _farmerBaseCustomised.GetPixels(0, 0, blockWidth, blockHeight);

            MergeColorArray(farmerBodyPixels, farmerTrousePixelsSelection);
            MergeColorArray(farmerBodyPixels, farmerShirtPixels);
            MergeColorArray(farmerBodyPixels, farmerAdornmentsPixels);

            _farmerBaseCustomised.SetPixels(0, 0, blockWidth, blockHeight, farmerBodyPixels);
            _farmerBaseCustomised.Apply();
        }
        #endregion

        #region Used For ProcessShirt()
        /// <summary>
        /// 填充身体朝向数组
        /// </summary>
        private void PopulateBodyFacingArray()
        {
            var facingData = new Facing[,] {
                // Row 0-9: None
                {Facing.None, Facing.None, Facing.None, Facing.None, Facing.None, Facing.None},
                {Facing.None, Facing.None, Facing.None, Facing.None, Facing.None, Facing.None},
                {Facing.None, Facing.None, Facing.None, Facing.None, Facing.None, Facing.None},
                {Facing.None, Facing.None, Facing.None, Facing.None, Facing.None, Facing.None},
                {Facing.None, Facing.None, Facing.None, Facing.None, Facing.None, Facing.None},
                {Facing.None, Facing.None, Facing.None, Facing.None, Facing.None, Facing.None},
                {Facing.None, Facing.None, Facing.None, Facing.None, Facing.None, Facing.None},
                {Facing.None, Facing.None, Facing.None, Facing.None, Facing.None, Facing.None},
                {Facing.None, Facing.None, Facing.None, Facing.None, Facing.None, Facing.None},
                {Facing.None, Facing.None, Facing.None, Facing.None, Facing.None, Facing.None},
                // Row 10-20: 其他方向
                {Facing.Back, Facing.Back, Facing.Right, Facing.Right, Facing.Right, Facing.Right},
                {Facing.Front, Facing.Front, Facing.Front, Facing.Front, Facing.Back, Facing.Back},
                {Facing.Back, Facing.Back, Facing.Right, Facing.Right, Facing.Right, Facing.Right},
                {Facing.Front, Facing.Front, Facing.Front, Facing.Front, Facing.Back, Facing.Back},
                {Facing.Back, Facing.Back, Facing.Right, Facing.Right, Facing.Right, Facing.Right},
                {Facing.Front, Facing.Front, Facing.Front, Facing.Front, Facing.Back, Facing.Back},
                {Facing.Back, Facing.Back, Facing.Right, Facing.Right, Facing.Right, Facing.Right},
                {Facing.Front, Facing.Front, Facing.Front, Facing.Front, Facing.Back, Facing.Back},
                {Facing.Back, Facing.Back, Facing.Back, Facing.Right, Facing.Right, Facing.Right},
                {Facing.Right, Facing.Right, Facing.Right, Facing.Front, Facing.Front, Facing.Front},
                {Facing.Front, Facing.Front, Facing.Front, Facing.Back, Facing.Back, Facing.Back}
            };

            for (int row = 0; row < _bodyRows; row++)
            {
                for (int col = 0; col < _bodyColumns; col++)
                {
                    _bodyFacingArray[col, row] = facingData[row, col];
                }
            }
        }

        /// <summary>
        /// 填充衣服偏移数组
        /// </summary>
        private void PopulateBodyShirtOffsetArray()
        {
            PopulateArrayWithDefaultValue(_bodyShirtOffsetArray, INVALID_OFFSET);

            var offsetData = new Vector2Int[,] {
                // Row 10
                {new Vector2Int(4, 11), new Vector2Int(4, 10), new Vector2Int(4, 11), new Vector2Int(4, 12), new Vector2Int(4, 11), new Vector2Int(4, 10)},
                // Row 11
                {new Vector2Int(4, 11), new Vector2Int(4, 12), new Vector2Int(4, 11), new Vector2Int(4, 10), new Vector2Int(4, 11), new Vector2Int(4, 12)},
                // Row 12
                {new Vector2Int(3, 9), new Vector2Int(3, 9), new Vector2Int(4, 10), new Vector2Int(4, 9), new Vector2Int(4, 9), new Vector2Int(4, 9)},
                // Row 13
                {new Vector2Int(4, 10), new Vector2Int(4, 9), new Vector2Int(5, 9), new Vector2Int(5, 9), new Vector2Int(4, 10), new Vector2Int(4, 9)},
                // Row 14
                {new Vector2Int(4, 9), new Vector2Int(4, 12), new Vector2Int(4, 7), new Vector2Int(4, 5), new Vector2Int(4, 9), new Vector2Int(4, 12)},
                // Row 15
                {new Vector2Int(4, 8), new Vector2Int(4, 5), new Vector2Int(4, 9), new Vector2Int(4, 12), new Vector2Int(4, 8), new Vector2Int(4, 5)},
                // Row 16
                {new Vector2Int(4, 9), new Vector2Int(4, 10), new Vector2Int(4, 7), new Vector2Int(4, 8), new Vector2Int(4, 9), new Vector2Int(4, 10)},
                // Row 17
                {new Vector2Int(4, 7), new Vector2Int(4, 8), new Vector2Int(4, 9), new Vector2Int(4, 10), new Vector2Int(4, 7), new Vector2Int(4, 8)},
                // Row 18
                {new Vector2Int(4, 10), new Vector2Int(4, 9), new Vector2Int(4, 9), new Vector2Int(4, 10), new Vector2Int(4, 9), new Vector2Int(4, 9)},
                // Row 19
                {new Vector2Int(4, 10), new Vector2Int(4, 9), new Vector2Int(4, 9), new Vector2Int(4, 10), new Vector2Int(4, 9), new Vector2Int(4, 9)},
                // Row 20
                {new Vector2Int(4, 10), new Vector2Int(4, 9), new Vector2Int(4, 9), new Vector2Int(4, 10), new Vector2Int(4, 9), new Vector2Int(4, 9)}
            };

            const int VALID_OFFSET_START_ROW = 10;
            for (int dataRow = 0; dataRow < offsetData.GetLength(0); dataRow++)
            {
                int actualRow = VALID_OFFSET_START_ROW + dataRow;
                for (int col = 0; col < _bodyColumns; col++)
                {
                    _bodyShirtOffsetArray[col, actualRow] = offsetData[dataRow, col];
                }
            }
        }

        /// <summary>
        /// 添加衬衫纹理到基础角色纹理
        /// </summary>
        /// <param name="shirtStyle">衬衫样式</param>
        private void AddShirtToTexture(int shirtStyle)
        {
            _selectedShirt = new(_shirtTextureWidth, _shirtTextureHeight)
            {
                filterMode = FilterMode.Point
            };

            int x = (shirtStyle % _shirtStyleInSpriteWidth) * _shirtTextureWidth;
            int y = (shirtStyle / _shirtStyleInSpriteWidth) * _shirtTextureHeight;

            var shirtPixels = _shirtBaseTexture.GetPixels(x, y, _shirtTextureWidth, _shirtTextureHeight);

            _selectedShirt.SetPixels(shirtPixels);
            _selectedShirt.Apply();
        }

        /// <summary>
        /// 应用衬衫纹理到基础角色纹理
        /// </summary>
        private void ApplyShirtTextureToBase()
        {
            InitializeShirtTexture();
            var shirtPixelData = PrepareShirtPixelData();
            ApplyShirtPixelsToTexture(shirtPixelData);
            _farmerBaseShirtUpdated.Apply();
        }

        /// <summary>
        /// 初始化衬衫更新纹理
        /// </summary>
        private void InitializeShirtTexture()
        {
            _farmerBaseShirtUpdated = new(_farmerBaseTexture.width, _farmerBaseTexture.height)
            {
                filterMode = FilterMode.Point
            };
            SetTextureToTransparent(_farmerBaseShirtUpdated);
        }

        /// <summary>
        /// 准备衬衫像素数据
        /// </summary>
        /// <returns>包含不同朝向衬衫像素的数据结构</returns>
        private ShirtPixelData PrepareShirtPixelData()
        {
            return new ShirtPixelData
            {
                FrontPixels = _selectedShirt.GetPixels(0, _shirtSpriteHeight * 3, _shirtSpriteWidth, _shirtSpriteHeight),
                BackPixels = _selectedShirt.GetPixels(0, _shirtSpriteHeight * 0, _shirtSpriteWidth, _shirtSpriteHeight),
                RightPixels = _selectedShirt.GetPixels(0, _shirtSpriteHeight * 2, _shirtSpriteWidth, _shirtSpriteHeight)
            };
        }

        /// <summary>
        /// 应用衬衫像素到纹理
        /// </summary>
        /// <param name="shirtPixelData">衬衫像素数据</param>
        private void ApplyShirtPixelsToTexture(ShirtPixelData shirtPixelData)
        {
            ApplyPixelsToTexture(shirtPixelData);
        }

        /// <summary>
        /// 通用的像素应用到纹理函数（衬衫重载）
        /// </summary>
        /// <param name="shirtPixelData">衬衫像素数据</param>
        private void ApplyPixelsToTexture(ShirtPixelData shirtPixelData)
        {
            for (int x = 0; x < _bodyColumns; x++)
            {
                for (int y = 0; y < _bodyRows; y++)
                {
                    ProcessSingleBodyPart(x, y, shirtPixelData);
                }
            }
        }

        /// <summary>
        /// 处理单个身体部分的衬衫应用
        /// </summary>
        /// <param name="x">列索引</param>
        /// <param name="y">行索引</param>
        /// <param name="shirtPixelData">衬衫像素数据</param>
        private void ProcessSingleBodyPart(int x, int y, ShirtPixelData shirtPixelData)
        {
            if (ShouldSkipBodyPart(x, y, BodyPartProcessType.Shirt))
                return;

            var facing = _bodyFacingArray[x, y];
            if (facing == Facing.None)
                return;

            var pixelPosition = CalculatePixelPosition(x, y, BodyPartProcessType.Shirt);
            ApplyPixelsByFacing(pixelPosition, facing, shirtPixelData);
        }

        /// <summary>
        /// 根据朝向应用对应的衬衫像素
        /// </summary>
        /// <param name="pixelPosition">像素位置</param>
        /// <param name="facing">朝向</param>
        /// <param name="shirtPixelData">衬衫像素数据</param>
        private void ApplyPixelsByFacing(Vector2Int pixelPosition, Facing facing, ShirtPixelData shirtPixelData)
        {
            Color[] pixels = facing switch
            {
                Facing.Front => shirtPixelData.FrontPixels,
                Facing.Back => shirtPixelData.BackPixels,
                Facing.Right => shirtPixelData.RightPixels,
                _ => null
            };

            if (pixels == null)
                return;

            _farmerBaseShirtUpdated.SetPixels(pixelPosition.x, pixelPosition.y, _shirtSpriteWidth, _shirtSpriteHeight, pixels);
        }
        #endregion

        #region Used For ProcessArms()
        /// <summary>
        /// 填充手臂颜色交换列表
        /// </summary>
        private void PopulateArmColorSwapList()
        {
            _colorSwapList.Clear();

            _colorSwapList.Add(new ColorSwap(_armTargetColor1, _selectedShirt.GetPixel(0, 7)));
            _colorSwapList.Add(new ColorSwap(_armTargetColor2, _selectedShirt.GetPixel(0, 6)));
            _colorSwapList.Add(new ColorSwap(_armTargetColor3, _selectedShirt.GetPixel(0, 5)));
        }
        #endregion

        #region Used For ProcessHair()
        /// <summary>
        /// 添加头发到纹理中
        /// </summary>
        /// <param name="hairStyle"></param>
        private void AddHairToTexture(int hairStyle)
        {
            int x = (hairStyle % _hairStyleInSpriteWidth) * _hairTextureWidth;
            int y = (hairStyle / _hairStyleInSpriteWidth) * _hairTextureHeight;

            var hairPixels = _hariBaseTexture.GetPixels(x, y, _hairTextureWidth, _hairTextureHeight);

            _hairCustomised.SetPixels(hairPixels);
            _hairCustomised.Apply();
        }
        #endregion

        #region Used For ProcessSkin()
        /// <summary>
        /// 填充皮肤颜色交换列表
        /// </summary>
        /// <param name="skinType">皮肤类型 (0-3)</param>
        private void PopulateSkinColorSwapList(int skinType)
        {
            _colorSwapList.Clear();

            var skinColors = GetSkinColorsForType(skinType);
            var targetColors = TargetSkinColors;

            for (int i = 0; i < targetColors.Length; i++)
            {
                _colorSwapList.Add(new ColorSwap(targetColors[i], skinColors[i]));
            }
        }

        /// <summary>
        /// 获取指定皮肤类型的颜色数据
        /// </summary>
        /// <param name="skinType">皮肤类型</param>
        /// <returns>对应的颜色数组</returns>
        private Color32[] GetSkinColorsForType(int skinType)
        {
            // 使用有效范围内的skinType，超出范围则使用默认值(0)
            int validSkinType = skinType >= 0 && skinType < _skinColorData.Length ? skinType : 0;
            return _skinColorData[validSkinType];
        }
        #endregion

        #region Used For ProcessHat()
        /// <summary>
        /// 添加帽子到纹理中
        /// </summary>
        /// <param name="hatStyle"></param>
        private void AddHatToTexture(int hatStyle)
        {
            int x = (hatStyle % _hatStyleInSpriteWidth) * _hatTextureWidth;
            int y = (hatStyle / _hatStyleInSpriteWidth) * _hatTextureHeight;

            var hatPixels = _hatBaseTexture.GetPixels(x, y, _hatTextureWidth, _hatTextureHeight);

            _hatCustomised.SetPixels(hatPixels);
            _hatCustomised.Apply();
        }
        #endregion

        #region Used For ProcessAdornments()
        /// <summary>
        /// 填充身体装饰偏移数组
        /// </summary>
        private void PopulateBodyAdornmentsOffsetArray()
        {
            PopulateArrayWithDefaultValue(_bodyAdornmentsOffsetArray, INVALID_OFFSET);

            var offsetData = new Vector2Int[,] {
                // Row 10
                {INVALID_OFFSET, INVALID_OFFSET, new Vector2Int(0, 17), new Vector2Int(0, 18), new Vector2Int(0, 17), new Vector2Int(0, 16)},
                // Row 11
                {new Vector2Int(0, 17), new Vector2Int(0, 18), new Vector2Int(0, 17), new Vector2Int(0, 16), INVALID_OFFSET, INVALID_OFFSET},
                // Row 12
                {INVALID_OFFSET, INVALID_OFFSET, new Vector2Int(0, 16), new Vector2Int(0, 15), new Vector2Int(0, 15), new Vector2Int(0, 15)},
                // Row 13
                {new Vector2Int(0, 16), new Vector2Int(0, 15), new Vector2Int(1, 15), new Vector2Int(1, 15), INVALID_OFFSET, INVALID_OFFSET},
                // Row 14
                {INVALID_OFFSET, INVALID_OFFSET, new Vector2Int(0, 13), new Vector2Int(0, 11), new Vector2Int(0, 15), new Vector2Int(0, 17)},
                // Row 15
                {new Vector2Int(0, 14), new Vector2Int(0, 11), new Vector2Int(0, 15), new Vector2Int(0, 18), INVALID_OFFSET, INVALID_OFFSET},
                // Row 16
                {INVALID_OFFSET, INVALID_OFFSET, new Vector2Int(0, 13), new Vector2Int(0, 14), new Vector2Int(0, 15), new Vector2Int(0, 16)},
                // Row 17
                {new Vector2Int(0, 13), new Vector2Int(0, 14), new Vector2Int(0, 15), new Vector2Int(0, 16), INVALID_OFFSET, INVALID_OFFSET},
                // Row 18
                {INVALID_OFFSET, INVALID_OFFSET, INVALID_OFFSET, new Vector2Int(0, 16), new Vector2Int(0, 16), new Vector2Int(0, 15)},
                // Row 19
                {new Vector2Int(0, 16), new Vector2Int(0, 15), new Vector2Int(0, 15), new Vector2Int(0, 16), new Vector2Int(0, 15), new Vector2Int(0, 15)},
                // Row 20
                {new Vector2Int(0, 16), new Vector2Int(0, 15), new Vector2Int(0, 15), INVALID_OFFSET, INVALID_OFFSET, INVALID_OFFSET},
            };

            const int VALID_OFFSET_START_ROW = 10;
            for (int dataRow = 0; dataRow < offsetData.GetLength(0); dataRow++)
            {
                int actualRow = VALID_OFFSET_START_ROW + dataRow;
                for (int col = 0; col < _bodyColumns; col++)
                {
                    _bodyAdornmentsOffsetArray[col, actualRow] = offsetData[dataRow, col];
                }
            }
        }

        /// <summary>
        /// 添加身体装饰到纹理中
        /// </summary>
        /// <param name="adornmentsStyle"></param>
        private void AddAdornmentsToTexture(int adornmentsStyle)
        {
            _selectedAdornments = new(_adornmentsTextureWidth, _adornmentsTextureHeight)
            {
                filterMode = FilterMode.Point
            };

            int x = (adornmentsStyle % _adornmentsStyleInSpriteWidth) * _adornmentsTextureWidth;
            int y = (adornmentsStyle / _adornmentsStyleInSpriteWidth) * _adornmentsTextureHeight;

            var adornmentsPixels = _adornmentsBaseTexture.GetPixels(x, y, _adornmentsTextureWidth, _adornmentsTextureHeight);

            _selectedAdornments.SetPixels(adornmentsPixels);
            _selectedAdornments.Apply();
        }

        /// <summary>
        /// 应用身体装饰纹理到基础角色纹理
        /// </summary>
        private void ApplyAdornmentsTextureToBase()
        {
            InitializeAdornmentsTexture();
            var adornmentsPixelData = PrepareAdornmentsPixelData();
            ApplyAdornmentsPixelsToTexture(adornmentsPixelData);
            _farmerBaseAdornmentsUpdated.Apply();
        }

        /// <summary>
        /// 初始化身体装饰更新纹理
        /// </summary>
        private void InitializeAdornmentsTexture()
        {
            _farmerBaseAdornmentsUpdated = new(_farmerBaseTexture.width, _farmerBaseTexture.height)
            {
                filterMode = FilterMode.Point
            };
            SetTextureToTransparent(_farmerBaseAdornmentsUpdated);
        }

        /// <summary>
        /// 准备身体装饰像素数据
        /// </summary>
        /// <returns>包含不同朝向身体装饰像素的数据结构</returns>
        private AdornmentsPixelData PrepareAdornmentsPixelData()
        {
            return new AdornmentsPixelData
            {
                FrontPixels = _selectedAdornments.GetPixels(0, _adornmentsSpriteHeight * 1, _adornmentsSpriteWidth, _adornmentsSpriteHeight),
                RightPixels = _selectedAdornments.GetPixels(0, _adornmentsSpriteHeight * 0, _adornmentsSpriteWidth, _adornmentsSpriteHeight)
            };
        }

        /// <summary>
        /// 应用身体装饰像素到纹理
        /// </summary>
        /// <param name="adornmentsPixelData">身体装饰像素数据</param>
        private void ApplyAdornmentsPixelsToTexture(AdornmentsPixelData adornmentsPixelData)
        {
            ApplyPixelsToTexture(adornmentsPixelData);
        }

        /// <summary>
        /// 通用的像素应用到纹理函数（装饰重载）
        /// </summary>
        /// <param name="adornmentsPixelData">装饰像素数据</param>
        private void ApplyPixelsToTexture(AdornmentsPixelData adornmentsPixelData)
        {
            for (int x = 0; x < _bodyColumns; x++)
            {
                for (int y = 0; y < _bodyRows; y++)
                {
                    ProcessSingleBodyPart(x, y, adornmentsPixelData);
                }
            }
        }

        /// <summary>
        /// 处理单个身体部分的装饰应用
        /// </summary>
        /// <param name="x">列索引</param>
        /// <param name="y">行索引</param>
        /// <param name="adornmentsPixelData">装饰像素数据</param>
        private void ProcessSingleBodyPart(int x, int y, AdornmentsPixelData adornmentsPixelData)
        {
            if (ShouldSkipBodyPart(x, y, BodyPartProcessType.Adornments))
                return;

            var facing = _bodyFacingArray[x, y];
            if (facing == Facing.None)
                return;

            var pixelPosition = CalculatePixelPosition(x, y, BodyPartProcessType.Adornments);
            ApplyPixelsByFacing(pixelPosition, facing, adornmentsPixelData);
        }

        /// <summary>
        /// 根据朝向应用对应的装饰像素
        /// </summary>
        /// <param name="pixelPosition">像素位置</param>
        /// <param name="facing">朝向</param>
        /// <param name="adornmentsPixelData">装饰像素数据</param>
        private void ApplyPixelsByFacing(Vector2Int pixelPosition, Facing facing, AdornmentsPixelData adornmentsPixelData)
        {
            Color[] pixels = facing switch
            {
                Facing.Front => adornmentsPixelData.FrontPixels,
                Facing.Right => adornmentsPixelData.RightPixels,
                _ => null
            };

            if (pixels == null)
                return;

            _farmerBaseAdornmentsUpdated.SetPixels(pixelPosition.x, pixelPosition.y, _adornmentsSpriteWidth, _adornmentsSpriteHeight, pixels);
        }
        #endregion

        #region Used For MergeCustomisation()
        /// <summary>
        /// 将两个颜色数组进行混合，根据alpha值实现颜色叠加效果
        /// </summary>
        /// <param name="baseArray">基础颜色数组，将被修改</param>
        /// <param name="mergeArray">要合并的颜色数组</param>
        private void MergeColorArray(Color[] baseArray, Color[] mergeArray)
        {
            if (baseArray == null || mergeArray == null)
                return;

            if (baseArray.Length != mergeArray.Length)
                return;

            for (int i = 0; i < baseArray.Length; i++)
            {
                ProcessPixelMerging(baseArray, mergeArray, i);
            }
        }

        /// <summary>
        /// 处理单个像素的颜色混合
        /// </summary>
        /// <param name="baseArray">基础颜色数组</param>
        /// <param name="mergeArray">要合并的颜色数组</param>
        /// <param name="index">像素索引</param>
        private void ProcessPixelMerging(Color[] baseArray, Color[] mergeArray, int index)
        {
            var mergeColor = mergeArray[index];

            if (mergeColor.a <= 0)
                return;

            // 完全不透明：直接替换
            if (mergeColor.a >= 1)
            {
                baseArray[index] = mergeColor;
                return;
            }

            // 半透明：执行Alpha混合
            BlendColors(ref baseArray[index], mergeColor);
        }

        /// <summary>
        /// 执行Alpha混合算法，将两个颜色进行混合
        /// </summary>
        /// <param name="baseColor">基础颜色，将被修改</param>
        /// <param name="mergeColor">要混合的颜色</param>
        private void BlendColors(ref Color baseColor, Color mergeColor)
        {
            float alpha = mergeColor.a;

            baseColor.r += (mergeColor.r - baseColor.r) * alpha;
            baseColor.g += (mergeColor.g - baseColor.g) * alpha;
            baseColor.b += (mergeColor.b - baseColor.b) * alpha;
            baseColor.a += mergeColor.a;
        }
        #endregion

        #region Tools Methods
        /// <summary>
        /// 使用指定值填充Vector2Int数组
        /// </summary>
        /// <param name="array">要填充的数组</param>
        /// <param name="value">填充值</param>
        private static void PopulateArrayWithDefaultValue(Vector2Int[,] array, Vector2Int value)
        {
            for (int x = 0; x < array.GetLength(0); x++)
            {
                for (int y = 0; y < array.GetLength(1); y++)
                {
                    array[x, y] = value;
                }
            }
        }

        /// <summary>
        /// 设置纹理为透明
        /// </summary>
        /// <param name="texture2D">要设置的纹理</param>
        private void SetTextureToTransparent(Texture2D texture2D)
        {
            Color[] pixels = texture2D.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }
            texture2D.SetPixels(pixels);
            texture2D.Apply();
        }

        /// <summary>
        /// 身体部分跳过检查函数
        /// </summary>
        /// <param name="x">列索引</param>
        /// <param name="y">行索引</param>
        /// <param name="processType">处理类型</param>
        /// <returns>如果应该跳过则返回true</returns>
        private bool ShouldSkipBodyPart(int x, int y, BodyPartProcessType processType)
        {
            Vector2Int offset = processType switch
            {
                BodyPartProcessType.Shirt => _bodyShirtOffsetArray[x, y],
                BodyPartProcessType.Adornments => _bodyAdornmentsOffsetArray[x, y],
                _ => INVALID_OFFSET
            };

            return offset.Equals(INVALID_OFFSET);
        }

        /// <summary>
        /// 像素位置计算函数
        /// </summary>
        /// <param name="x">列索引</param>
        /// <param name="y">行索引</param>
        /// <param name="processType">处理类型</param>
        /// <returns>计算后的像素位置</returns>
        private Vector2Int CalculatePixelPosition(int x, int y, BodyPartProcessType processType)
        {
            int pixelX = x * _farmerSpriteWidth;
            int pixelY = y * _farmerSpriteHeight;

            Vector2Int offset = processType switch
            {
                BodyPartProcessType.Shirt => _bodyShirtOffsetArray[x, y],
                BodyPartProcessType.Adornments => _bodyAdornmentsOffsetArray[x, y],
                _ => INVALID_OFFSET
            };

            if (!offset.Equals(INVALID_OFFSET))
            {
                pixelX += offset.x;
                pixelY += offset.y;
            }

            return new Vector2Int(pixelX, pixelY);
        }

        /// <summary>
        /// 改变像素颜色，将baseArray中匹配originalColor的像素替换为newColor
        /// </summary>
        /// <param name="baseArray">要处理的像素数组</param>
        /// <param name="colorSwapList">颜色交换列表</param>
        private void ChangePixelsColor(Color[] baseArray, List<ColorSwap> colorSwapList)
        {
            if (baseArray == null || colorSwapList == null)
                return;

            for (int i = 0; i < baseArray.Length; i++)
            {
                var currentPixel = baseArray[i];
                var swappedColor = FindMatchingColorSwap(currentPixel, colorSwapList);

                if (swappedColor.HasValue)
                    baseArray[i] = swappedColor.Value;
            }
        }

        /// <summary>
        /// 在颜色交换列表中查找匹配的颜色
        /// </summary>
        /// <param name="targetColor">要查找的目标颜色</param>
        /// <param name="colorSwapList">颜色交换列表</param>
        /// <returns>如果找到匹配的颜色返回新颜色，否则返回null</returns>
        private Color? FindMatchingColorSwap(Color targetColor, List<ColorSwap> colorSwapList)
        {
            foreach (var colorSwap in colorSwapList)
            {
                if (colorSwap == null)
                    continue;

                if (IsSameColor(targetColor, colorSwap.originalColor))
                    return colorSwap.newColor;
            }

            return null;
        }

        /// <summary>
        /// 比较两个颜色是否相同（包括Alpha通道）
        /// </summary>
        /// <param name="color1">第一个颜色</param>
        /// <param name="color2">第二个颜色</param>
        /// <returns>如果颜色相同返回true，否则返回false</returns>
        private bool IsSameColor(Color color1, Color color2)
        {
            return color1 == color2;
        }

        /// <summary>
        /// 为像素染色
        /// </summary>
        /// <param name="basePixelArray"></param>
        /// <param name="tintColor"></param>
        private void TintPixelColors(Color[] basePixelArray, Color tintColor)
        {
            for (int i = 0; i < basePixelArray.Length; i++)
            {
                basePixelArray[i].r *= tintColor.r;
                basePixelArray[i].g *= tintColor.g;
                basePixelArray[i].b *= tintColor.b;
            }
        }
        #endregion
    }
}