using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace CollectionEditorEx
{
    /// <summary>
    /// 拡張コレクションエディタ
    /// </summary>
    /// <remarks>このエディタで行っていること
    /// 1. リストアイテムの追加、削除、移動の禁止
    /// 2. プロパティラベルの文字列変更
    /// 3. フォームクローズ時にタイトルが重複していればエラーメッセージを通知する
    /// </remarks>
    public class CollectionEditorEx : CollectionEditor
    {
        /// <summary>
        /// リストアイテム変更コントロール有効フラグ
        /// </summary>
        protected bool _isEnableListItemControl { get; set; }

        /// <summary>
        /// コレクションフォーム
        /// </summary>
        protected CollectionForm _collectionForm { get; set; }

        /// <summary>
        /// フォームコントロールテーブル
        /// </summary>
        protected Dictionary<string, Control> _formControlTable { get; set; } = new Dictionary<string, Control>();

        /// <summary>
        /// リストボックス
        /// </summary>
        protected ListBox _listBox { get; set; }

        /// <summary>
        /// プロパティグリッド
        /// </summary>
        protected PropertyGrid _propertyGrid { get; set; }

        /// <summary>
        /// OKボタンクリックフラグ
        /// </summary>
        protected bool _clickedOkButton { get; set; }

        /// <summary>
        /// 変種後アイテムリスト
        /// </summary>
        protected List<object> _editedItems { get; set; } = new List<object>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="type"></param>
        public CollectionEditorEx(Type type) : base(type)
        {
        }

        /// <summary>
        /// コレクションフォーム作成
        /// </summary>
        /// <returns></returns>
        protected override CollectionForm CreateCollectionForm()
        {
            _collectionForm = base.CreateCollectionForm();
            _collectionForm.Load += Form_Load;
            _collectionForm.FormClosing += Form_FormClosing;
            return _collectionForm;
        }

        /// <summary>
        /// 指定した配列をコレクション内の項目として設定します。
        /// </summary>
        /// <param name="editValue"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override object SetItems(object editValue, object[] value)
        {
            // 編集結果を取得してクローズ時に検証する
            _editedItems.Clear();
            _editedItems.AddRange(value);

            return base.SetItems(editValue, value);
        }

        /// <summary>
        /// フォームのロードイベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Form_Load(object sender, EventArgs e)
        {
            OnLoad(sender, e);
        }

        /// <summary>
        /// フォームロード時に呼ばれる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnLoad(object sender, EventArgs e)
        {
            InitFormControlTable();
            CustomizeFormControlBehavior();
        }

        /// <summary>
        /// フォームのクローズ処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            OnClosing(sender, e);
        }

        /// <summary>
        /// フォームクローズ時に呼ばれる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnClosing(object sender, FormClosingEventArgs e)
        {
            List<string> tempItems = new List<string>();
            foreach(object item in _editedItems)
            {
                if (tempItems.Contains(item as string))
                {
                    MessageBox.Show("タイトルが重複しています");
                    break;
                }
                tempItems.Add(item as string);
            }
        }

        /// <summary>
        /// フォームコントロールテーブルの初期化
        /// </summary>
        private void InitFormControlTable()
        {
            _formControlTable.Clear();
            foreach (Control child in _collectionForm.Controls)
            {
                InitFormControlTableRecursive(child);
            }
        }

        /// <summary>
        /// フォームコントロールテーブルの初期化（再帰）
        /// </summary>
        /// <param name="control"></param>
        private void InitFormControlTableRecursive(Control control)
        {
            if (string.IsNullOrEmpty(control.Name) == false)
            {
                _formControlTable.Add(control.Name, control);
            }
            foreach (Control child in control.Controls)
            {
                InitFormControlTableRecursive(child);
            }
        }

        /// <summary>
        /// フォームコントロールの振る舞いをカスタマイズする
        /// </summary>
        protected virtual void CustomizeFormControlBehavior()
        {
            foreach (KeyValuePair<string, Control> kvp in _formControlTable)
            {
                kvp.Value.TextChanged += Control_TextChanged;
                kvp.Value.Click += Control_Click;
                switch (kvp.Value.Name)
                {
                    case "listbox":
                        _listBox = kvp.Value as ListBox;
                        break;
                    case "propertyBrowser":
                        _propertyGrid = kvp.Value as PropertyGrid;
                        _propertyGrid.PropertyValueChanged += _propertyGrid_PropertyValueChanged;
                        break;
                }
            }

            CustomizeListItemControl();
        }

        /// <summary>
        /// リストアイテム変更コントロールをカスタマイズする
        /// </summary>
        protected virtual void CustomizeListItemControl()
        {
            if (_isEnableListItemControl)
            {
                return;
            }

            foreach (KeyValuePair<string, Control> kvp in _formControlTable)
            {
                switch (kvp.Value.Name)
                {
                    case "addButton":
                    case "removeButton":
                    case "upButton":
                    case "downButton":
                        kvp.Value.EnabledChanged += Control_EnabledChanged;
                        kvp.Value.Enabled = false;
                        break;
                }
            }
        }

        /// <summary>
        /// コントロール有効状態変更イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_EnabledChanged(object sender, EventArgs e)
        {
            Control control = sender as Control;
            if (control == null)
            {
                return;
            }
            OnControlEnableChanged(control, e);
        }

        /// <summary>
        /// コントロール有効状態変更時に呼ばれる
        /// </summary>
        /// <param name="control"></param>
        /// <param name="e"></param>
        protected virtual void OnControlEnableChanged(Control control, EventArgs e)
        {
            switch (control.Name)
            {
                case "addButton":
                case "removeButton":
                case "upButton":
                case "downButton":
                    control.Enabled = false;
                    break;
            }
        }

        /// <summary>
        /// プロパティラベルテキスト変更イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_TextChanged(object sender, EventArgs e)
        {
            Control control = sender as Control;
            if (control == null)
            {
                return;
            }
            OnControlTextChanged(control, e);
        }

        /// <summary>
        /// プロパティラベルテキスト変更時に呼ばれる
        /// </summary>
        /// <param name="control"></param>
        /// <param name="e"></param>
        protected virtual void OnControlTextChanged(Control control, EventArgs e)
        {
            switch (control.Name)
            {
                case "propertiesLabel":
                    control.Text = "プロパティ(&P):";
                    break;
            }
        }

        /// <summary>
        /// コントロールクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_Click(object sender, EventArgs e)
        {
            Control control = sender as Control;
            if (control == null)
            {
                return;
            }
            OnControlClick(control, e);
        }

        /// <summary>
        /// コントロールクリック時に呼ばれる
        /// </summary>
        /// <param name="control"></param>
        /// <param name="e"></param>
        protected virtual void OnControlClick(Control control, EventArgs e)
        {
            switch (control.Name)
            {
                case "okButton":
                    _clickedOkButton = true;
                    break;
            }
        }

        /// <summary>
        /// プロパティグリッド値変更イベント
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void _propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            PropertyGrid grid = s as PropertyGrid;
            if (grid == null)
            {
                return;
            }
            OnPropertyGirdValueChanged(grid, e);
        }

        /// <summary>
        /// プロパティグリッド値変更処理
        /// </summary>
        /// <param name="gird"></param>
        /// <param name="e"></param>
        protected virtual void OnPropertyGirdValueChanged(PropertyGrid gird, PropertyValueChangedEventArgs e)
        {
        }

    }
}
