namespace MySQL_Clear_standart
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.checkBox_Tab2_ClusterixN_Online = new System.Windows.Forms.CheckBox();
            this.comboBox_tab2_IP = new System.Windows.Forms.ComboBox();
            this.checkBox_Tab2_ClusterXNEnable = new System.Windows.Forms.CheckBox();
            this.checkBox_tab2_DisableHeavyQuerry = new System.Windows.Forms.CheckBox();
            this.textBox_tab2_AllResult = new System.Windows.Forms.TextBox();
            this.btn_tab2_CreateAll = new System.Windows.Forms.Button();
            this.btn_tab2_CreateSort = new System.Windows.Forms.Button();
            this.textBox_tab2_SortResult = new System.Windows.Forms.TextBox();
            this.textBox_tab2_JoinResult = new System.Windows.Forms.TextBox();
            this.btn_tab2_CreateJoin = new System.Windows.Forms.Button();
            this.comboBox_tab2_QueryNumber = new System.Windows.Forms.ComboBox();
            this.btn_tab2_SelectQuery = new System.Windows.Forms.Button();
            this.textBox_tab2_SelectResult = new System.Windows.Forms.TextBox();
            this.textBox_tab2_Query = new System.Windows.Forms.TextBox();
            this.btn_tab2_CreateSelect = new System.Windows.Forms.Button();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panel_tab1_main = new System.Windows.Forms.Panel();
            this.richTextBox_tab1_Query = new System.Windows.Forms.RichTextBox();
            this.checkBox_tab1_DisableHeavyQuerry = new System.Windows.Forms.CheckBox();
            this.comboBox_tab1_QueryNumber = new System.Windows.Forms.ComboBox();
            this.btn_tab1_Debug = new System.Windows.Forms.Button();
            this.btn_tab1_SelectQuerry = new System.Windows.Forms.Button();
            this.btn_tab1_SaveTree = new System.Windows.Forms.Button();
            this.pictureBox_tab1_Tree = new System.Windows.Forms.PictureBox();
            this.btn_tab1_CreateTree = new System.Windows.Forms.Button();
            this.textBox_tab1_Query = new System.Windows.Forms.TextBox();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.richTextBox_tab4_XML = new System.Windows.Forms.RichTextBox();
            this.btn_tab4_SendToClusterix = new System.Windows.Forms.Button();
            this.comboBox_tab4_connetionIP = new System.Windows.Forms.ComboBox();
            this.tabPage3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel_tab1_main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_tab1_Tree)).BeginInit();
            this.tabControl_main.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.textBox4);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(1128, 557);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "TO DO";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // textBox4
            // 
            this.textBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox4.Location = new System.Drawing.Point(6, 6);
            this.textBox4.Multiline = true;
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(545, 406);
            this.textBox4.TabIndex = 0;
            this.textBox4.Text = resources.GetString("textBox4.Text");
            this.textBox4.TextChanged += new System.EventHandler(this.textBox4_TextChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.checkBox_Tab2_ClusterixN_Online);
            this.tabPage2.Controls.Add(this.comboBox_tab2_IP);
            this.tabPage2.Controls.Add(this.checkBox_Tab2_ClusterXNEnable);
            this.tabPage2.Controls.Add(this.checkBox_tab2_DisableHeavyQuerry);
            this.tabPage2.Controls.Add(this.textBox_tab2_AllResult);
            this.tabPage2.Controls.Add(this.btn_tab2_CreateAll);
            this.tabPage2.Controls.Add(this.btn_tab2_CreateSort);
            this.tabPage2.Controls.Add(this.textBox_tab2_SortResult);
            this.tabPage2.Controls.Add(this.textBox_tab2_JoinResult);
            this.tabPage2.Controls.Add(this.btn_tab2_CreateJoin);
            this.tabPage2.Controls.Add(this.comboBox_tab2_QueryNumber);
            this.tabPage2.Controls.Add(this.btn_tab2_SelectQuery);
            this.tabPage2.Controls.Add(this.textBox_tab2_SelectResult);
            this.tabPage2.Controls.Add(this.textBox_tab2_Query);
            this.tabPage2.Controls.Add(this.btn_tab2_CreateSelect);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1128, 557);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // checkBox_Tab2_ClusterixN_Online
            // 
            this.checkBox_Tab2_ClusterixN_Online.AutoSize = true;
            this.checkBox_Tab2_ClusterixN_Online.Location = new System.Drawing.Point(669, 10);
            this.checkBox_Tab2_ClusterixN_Online.Name = "checkBox_Tab2_ClusterixN_Online";
            this.checkBox_Tab2_ClusterixN_Online.Size = new System.Drawing.Size(109, 17);
            this.checkBox_Tab2_ClusterixN_Online.TabIndex = 18;
            this.checkBox_Tab2_ClusterixN_Online.Text = "ClusterixN_Online";
            this.checkBox_Tab2_ClusterixN_Online.UseVisualStyleBackColor = true;
            // 
            // comboBox_tab2_IP
            // 
            this.comboBox_tab2_IP.FormattingEnabled = true;
            this.comboBox_tab2_IP.Items.AddRange(new object[] {
            "10.114.20.200",
            "127.0.0.1"});
            this.comboBox_tab2_IP.Location = new System.Drawing.Point(531, 34);
            this.comboBox_tab2_IP.Name = "comboBox_tab2_IP";
            this.comboBox_tab2_IP.Size = new System.Drawing.Size(121, 21);
            this.comboBox_tab2_IP.TabIndex = 17;
            this.comboBox_tab2_IP.Text = "127.0.0.1";
            this.comboBox_tab2_IP.TextChanged += new System.EventHandler(this.comboBox_tab2_IP_TextChanged);
            // 
            // checkBox_Tab2_ClusterXNEnable
            // 
            this.checkBox_Tab2_ClusterXNEnable.AutoSize = true;
            this.checkBox_Tab2_ClusterXNEnable.Location = new System.Drawing.Point(531, 10);
            this.checkBox_Tab2_ClusterXNEnable.Name = "checkBox_Tab2_ClusterXNEnable";
            this.checkBox_Tab2_ClusterXNEnable.Size = new System.Drawing.Size(134, 17);
            this.checkBox_Tab2_ClusterXNEnable.TabIndex = 16;
            this.checkBox_Tab2_ClusterXNEnable.Text = "Create XML for Cluterix";
            this.checkBox_Tab2_ClusterXNEnable.UseVisualStyleBackColor = true;
            // 
            // checkBox_tab2_DisableHeavyQuerry
            // 
            this.checkBox_tab2_DisableHeavyQuerry.AutoSize = true;
            this.checkBox_tab2_DisableHeavyQuerry.Location = new System.Drawing.Point(47, 10);
            this.checkBox_tab2_DisableHeavyQuerry.Name = "checkBox_tab2_DisableHeavyQuerry";
            this.checkBox_tab2_DisableHeavyQuerry.Size = new System.Drawing.Size(100, 17);
            this.checkBox_tab2_DisableHeavyQuerry.TabIndex = 15;
            this.checkBox_tab2_DisableHeavyQuerry.Text = "DisableHeavyQ";
            this.checkBox_tab2_DisableHeavyQuerry.UseVisualStyleBackColor = true;
            this.checkBox_tab2_DisableHeavyQuerry.CheckedChanged += new System.EventHandler(this.checkBox_tab2_DisableHeavyQuerry_CheckedChanged);
            // 
            // textBox_tab2_AllResult
            // 
            this.textBox_tab2_AllResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_tab2_AllResult.Location = new System.Drawing.Point(845, 83);
            this.textBox_tab2_AllResult.Multiline = true;
            this.textBox_tab2_AllResult.Name = "textBox_tab2_AllResult";
            this.textBox_tab2_AllResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_tab2_AllResult.Size = new System.Drawing.Size(204, 451);
            this.textBox_tab2_AllResult.TabIndex = 14;
            this.textBox_tab2_AllResult.KeyDown += new System.Windows.Forms.KeyEventHandler(this.allow_SelectAl);
            // 
            // btn_tab2_CreateAll
            // 
            this.btn_tab2_CreateAll.Location = new System.Drawing.Point(449, 6);
            this.btn_tab2_CreateAll.Name = "btn_tab2_CreateAll";
            this.btn_tab2_CreateAll.Size = new System.Drawing.Size(75, 71);
            this.btn_tab2_CreateAll.TabIndex = 13;
            this.btn_tab2_CreateAll.Text = "Составить запросы на проверку";
            this.btn_tab2_CreateAll.UseVisualStyleBackColor = true;
            this.btn_tab2_CreateAll.Click += new System.EventHandler(this.btn_CreateTest_Click);
            // 
            // btn_tab2_CreateSort
            // 
            this.btn_tab2_CreateSort.Location = new System.Drawing.Point(368, 6);
            this.btn_tab2_CreateSort.Name = "btn_tab2_CreateSort";
            this.btn_tab2_CreateSort.Size = new System.Drawing.Size(75, 71);
            this.btn_tab2_CreateSort.TabIndex = 12;
            this.btn_tab2_CreateSort.Text = "Составить запросы SORT";
            this.btn_tab2_CreateSort.UseVisualStyleBackColor = true;
            this.btn_tab2_CreateSort.Click += new System.EventHandler(this.btn_CreateSort_Click);
            // 
            // textBox_tab2_SortResult
            // 
            this.textBox_tab2_SortResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_tab2_SortResult.Location = new System.Drawing.Point(635, 83);
            this.textBox_tab2_SortResult.Multiline = true;
            this.textBox_tab2_SortResult.Name = "textBox_tab2_SortResult";
            this.textBox_tab2_SortResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_tab2_SortResult.Size = new System.Drawing.Size(204, 451);
            this.textBox_tab2_SortResult.TabIndex = 11;
            this.textBox_tab2_SortResult.KeyDown += new System.Windows.Forms.KeyEventHandler(this.allow_SelectAl);
            // 
            // textBox_tab2_JoinResult
            // 
            this.textBox_tab2_JoinResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_tab2_JoinResult.Location = new System.Drawing.Point(215, 83);
            this.textBox_tab2_JoinResult.Multiline = true;
            this.textBox_tab2_JoinResult.Name = "textBox_tab2_JoinResult";
            this.textBox_tab2_JoinResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_tab2_JoinResult.Size = new System.Drawing.Size(204, 451);
            this.textBox_tab2_JoinResult.TabIndex = 10;
            this.textBox_tab2_JoinResult.KeyDown += new System.Windows.Forms.KeyEventHandler(this.allow_SelectAl);
            // 
            // btn_tab2_CreateJoin
            // 
            this.btn_tab2_CreateJoin.Location = new System.Drawing.Point(287, 6);
            this.btn_tab2_CreateJoin.Name = "btn_tab2_CreateJoin";
            this.btn_tab2_CreateJoin.Size = new System.Drawing.Size(75, 71);
            this.btn_tab2_CreateJoin.TabIndex = 9;
            this.btn_tab2_CreateJoin.Text = "Составить запросы JOIN";
            this.btn_tab2_CreateJoin.UseVisualStyleBackColor = true;
            this.btn_tab2_CreateJoin.Click += new System.EventHandler(this.btn_CreateJoin_Click);
            // 
            // comboBox_tab2_QueryNumber
            // 
            this.comboBox_tab2_QueryNumber.FormattingEnabled = true;
            this.comboBox_tab2_QueryNumber.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14"});
            this.comboBox_tab2_QueryNumber.Location = new System.Drawing.Point(8, 6);
            this.comboBox_tab2_QueryNumber.Name = "comboBox_tab2_QueryNumber";
            this.comboBox_tab2_QueryNumber.Size = new System.Drawing.Size(33, 21);
            this.comboBox_tab2_QueryNumber.TabIndex = 8;
            this.comboBox_tab2_QueryNumber.Text = "1";
            // 
            // btn_tab2_SelectQuery
            // 
            this.btn_tab2_SelectQuery.Location = new System.Drawing.Point(8, 37);
            this.btn_tab2_SelectQuery.Name = "btn_tab2_SelectQuery";
            this.btn_tab2_SelectQuery.Size = new System.Drawing.Size(93, 40);
            this.btn_tab2_SelectQuery.TabIndex = 5;
            this.btn_tab2_SelectQuery.Text = "Выбрать запрос";
            this.btn_tab2_SelectQuery.UseVisualStyleBackColor = true;
            this.btn_tab2_SelectQuery.Click += new System.EventHandler(this.btn_SelectQuerry_tab2_Click);
            // 
            // textBox_tab2_SelectResult
            // 
            this.textBox_tab2_SelectResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_tab2_SelectResult.Location = new System.Drawing.Point(425, 83);
            this.textBox_tab2_SelectResult.Multiline = true;
            this.textBox_tab2_SelectResult.Name = "textBox_tab2_SelectResult";
            this.textBox_tab2_SelectResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_tab2_SelectResult.Size = new System.Drawing.Size(204, 451);
            this.textBox_tab2_SelectResult.TabIndex = 2;
            this.textBox_tab2_SelectResult.KeyDown += new System.Windows.Forms.KeyEventHandler(this.allow_SelectAl);
            // 
            // textBox_tab2_Query
            // 
            this.textBox_tab2_Query.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_tab2_Query.Location = new System.Drawing.Point(6, 83);
            this.textBox_tab2_Query.Margin = new System.Windows.Forms.Padding(3, 3, 50, 3);
            this.textBox_tab2_Query.Multiline = true;
            this.textBox_tab2_Query.Name = "textBox_tab2_Query";
            this.textBox_tab2_Query.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.textBox_tab2_Query.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_tab2_Query.Size = new System.Drawing.Size(204, 466);
            this.textBox_tab2_Query.TabIndex = 1;
            this.textBox_tab2_Query.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            this.textBox_tab2_Query.KeyDown += new System.Windows.Forms.KeyEventHandler(this.allow_SelectAl);
            // 
            // btn_tab2_CreateSelect
            // 
            this.btn_tab2_CreateSelect.Location = new System.Drawing.Point(206, 6);
            this.btn_tab2_CreateSelect.Name = "btn_tab2_CreateSelect";
            this.btn_tab2_CreateSelect.Size = new System.Drawing.Size(75, 71);
            this.btn_tab2_CreateSelect.TabIndex = 0;
            this.btn_tab2_CreateSelect.Text = "Составить запросы SELECT";
            this.btn_tab2_CreateSelect.UseVisualStyleBackColor = true;
            this.btn_tab2_CreateSelect.Click += new System.EventHandler(this.btn_CreateSelect_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.panel_tab1_main);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1128, 557);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // panel_tab1_main
            // 
            this.panel_tab1_main.AutoScroll = true;
            this.panel_tab1_main.AutoSize = true;
            this.panel_tab1_main.Controls.Add(this.richTextBox_tab1_Query);
            this.panel_tab1_main.Controls.Add(this.checkBox_tab1_DisableHeavyQuerry);
            this.panel_tab1_main.Controls.Add(this.comboBox_tab1_QueryNumber);
            this.panel_tab1_main.Controls.Add(this.btn_tab1_Debug);
            this.panel_tab1_main.Controls.Add(this.btn_tab1_SelectQuerry);
            this.panel_tab1_main.Controls.Add(this.btn_tab1_SaveTree);
            this.panel_tab1_main.Controls.Add(this.pictureBox_tab1_Tree);
            this.panel_tab1_main.Controls.Add(this.btn_tab1_CreateTree);
            this.panel_tab1_main.Controls.Add(this.textBox_tab1_Query);
            this.panel_tab1_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_tab1_main.Location = new System.Drawing.Point(3, 3);
            this.panel_tab1_main.Name = "panel_tab1_main";
            this.panel_tab1_main.Size = new System.Drawing.Size(1122, 551);
            this.panel_tab1_main.TabIndex = 0;
            // 
            // richTextBox_tab1_Query
            // 
            this.richTextBox_tab1_Query.Location = new System.Drawing.Point(293, 91);
            this.richTextBox_tab1_Query.Name = "richTextBox_tab1_Query";
            this.richTextBox_tab1_Query.ReadOnly = true;
            this.richTextBox_tab1_Query.Size = new System.Drawing.Size(551, 412);
            this.richTextBox_tab1_Query.TabIndex = 10;
            this.richTextBox_tab1_Query.Text = "";
            this.richTextBox_tab1_Query.Visible = false;
            // 
            // checkBox_tab1_DisableHeavyQuerry
            // 
            this.checkBox_tab1_DisableHeavyQuerry.AutoSize = true;
            this.checkBox_tab1_DisableHeavyQuerry.Location = new System.Drawing.Point(188, 30);
            this.checkBox_tab1_DisableHeavyQuerry.Name = "checkBox_tab1_DisableHeavyQuerry";
            this.checkBox_tab1_DisableHeavyQuerry.Size = new System.Drawing.Size(100, 17);
            this.checkBox_tab1_DisableHeavyQuerry.TabIndex = 9;
            this.checkBox_tab1_DisableHeavyQuerry.Text = "DisableHeavyQ";
            this.checkBox_tab1_DisableHeavyQuerry.UseVisualStyleBackColor = true;
            // 
            // comboBox_tab1_QueryNumber
            // 
            this.comboBox_tab1_QueryNumber.FormattingEnabled = true;
            this.comboBox_tab1_QueryNumber.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14"});
            this.comboBox_tab1_QueryNumber.Location = new System.Drawing.Point(185, 3);
            this.comboBox_tab1_QueryNumber.Name = "comboBox_tab1_QueryNumber";
            this.comboBox_tab1_QueryNumber.Size = new System.Drawing.Size(67, 21);
            this.comboBox_tab1_QueryNumber.TabIndex = 7;
            this.comboBox_tab1_QueryNumber.Text = "1";
            // 
            // btn_tab1_Debug
            // 
            this.btn_tab1_Debug.Location = new System.Drawing.Point(86, 49);
            this.btn_tab1_Debug.Name = "btn_tab1_Debug";
            this.btn_tab1_Debug.Size = new System.Drawing.Size(93, 36);
            this.btn_tab1_Debug.TabIndex = 5;
            this.btn_tab1_Debug.Text = "Отладка";
            this.btn_tab1_Debug.UseVisualStyleBackColor = true;
            this.btn_tab1_Debug.Click += new System.EventHandler(this.btn_Debug_Click);
            // 
            // btn_tab1_SelectQuerry
            // 
            this.btn_tab1_SelectQuerry.Location = new System.Drawing.Point(5, 3);
            this.btn_tab1_SelectQuerry.Name = "btn_tab1_SelectQuerry";
            this.btn_tab1_SelectQuerry.Size = new System.Drawing.Size(93, 40);
            this.btn_tab1_SelectQuerry.TabIndex = 4;
            this.btn_tab1_SelectQuerry.Text = "Выбрать запрос";
            this.btn_tab1_SelectQuerry.UseVisualStyleBackColor = true;
            this.btn_tab1_SelectQuerry.Click += new System.EventHandler(this.btn_SelectQuerry_tab1_Click);
            // 
            // btn_tab1_SaveTree
            // 
            this.btn_tab1_SaveTree.Location = new System.Drawing.Point(5, 49);
            this.btn_tab1_SaveTree.Name = "btn_tab1_SaveTree";
            this.btn_tab1_SaveTree.Size = new System.Drawing.Size(75, 36);
            this.btn_tab1_SaveTree.TabIndex = 3;
            this.btn_tab1_SaveTree.Text = "Сохранить дерево";
            this.btn_tab1_SaveTree.UseVisualStyleBackColor = true;
            this.btn_tab1_SaveTree.Click += new System.EventHandler(this.btn_SaveTree_Click);
            // 
            // pictureBox_tab1_Tree
            // 
            this.pictureBox_tab1_Tree.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_tab1_Tree.Image")));
            this.pictureBox_tab1_Tree.Location = new System.Drawing.Point(294, -3);
            this.pictureBox_tab1_Tree.Name = "pictureBox_tab1_Tree";
            this.pictureBox_tab1_Tree.Size = new System.Drawing.Size(175, 159);
            this.pictureBox_tab1_Tree.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_tab1_Tree.TabIndex = 2;
            this.pictureBox_tab1_Tree.TabStop = false;
            // 
            // btn_tab1_CreateTree
            // 
            this.btn_tab1_CreateTree.Location = new System.Drawing.Point(104, 3);
            this.btn_tab1_CreateTree.Name = "btn_tab1_CreateTree";
            this.btn_tab1_CreateTree.Size = new System.Drawing.Size(75, 40);
            this.btn_tab1_CreateTree.TabIndex = 1;
            this.btn_tab1_CreateTree.Text = "Построить дерево";
            this.btn_tab1_CreateTree.UseVisualStyleBackColor = true;
            this.btn_tab1_CreateTree.Click += new System.EventHandler(this.btn_CreateTree_Click);
            // 
            // textBox_tab1_Query
            // 
            this.textBox_tab1_Query.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_tab1_Query.Location = new System.Drawing.Point(5, 91);
            this.textBox_tab1_Query.Multiline = true;
            this.textBox_tab1_Query.Name = "textBox_tab1_Query";
            this.textBox_tab1_Query.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_tab1_Query.Size = new System.Drawing.Size(283, 413);
            this.textBox_tab1_Query.TabIndex = 0;
            this.textBox_tab1_Query.Text = resources.GetString("textBox_tab1_Query.Text");
            this.textBox_tab1_Query.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.textBox_tab1_Query.KeyDown += new System.Windows.Forms.KeyEventHandler(this.allow_SelectAl);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Controls.Add(this.tabPage1);
            this.tabControl_main.Controls.Add(this.tabPage2);
            this.tabControl_main.Controls.Add(this.tabPage3);
            this.tabControl_main.Controls.Add(this.tabPage4);
            this.tabControl_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_main.Location = new System.Drawing.Point(0, 0);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(1136, 583);
            this.tabControl_main.TabIndex = 5;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.comboBox_tab4_connetionIP);
            this.tabPage4.Controls.Add(this.btn_tab4_SendToClusterix);
            this.tabPage4.Controls.Add(this.richTextBox_tab4_XML);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(1128, 557);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "tabPage4";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // richTextBox_tab4_XML
            // 
            this.richTextBox_tab4_XML.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.richTextBox_tab4_XML.Location = new System.Drawing.Point(3, 6);
            this.richTextBox_tab4_XML.Name = "richTextBox_tab4_XML";
            this.richTextBox_tab4_XML.Size = new System.Drawing.Size(342, 543);
            this.richTextBox_tab4_XML.TabIndex = 12;
            this.richTextBox_tab4_XML.Text = "";
            // 
            // btn_tab4_SendToClusterix
            // 
            this.btn_tab4_SendToClusterix.Location = new System.Drawing.Point(351, 6);
            this.btn_tab4_SendToClusterix.Name = "btn_tab4_SendToClusterix";
            this.btn_tab4_SendToClusterix.Size = new System.Drawing.Size(121, 23);
            this.btn_tab4_SendToClusterix.TabIndex = 13;
            this.btn_tab4_SendToClusterix.Text = "Отправить в Clusterix";
            this.btn_tab4_SendToClusterix.UseVisualStyleBackColor = true;
            this.btn_tab4_SendToClusterix.Click += new System.EventHandler(this.btn_tab4_SendToClusterix_Click);
            // 
            // comboBox_tab4_connetionIP
            // 
            this.comboBox_tab4_connetionIP.FormattingEnabled = true;
            this.comboBox_tab4_connetionIP.Items.AddRange(new object[] {
            "10.114.20.200",
            "127.0.0.1"});
            this.comboBox_tab4_connetionIP.Location = new System.Drawing.Point(351, 35);
            this.comboBox_tab4_connetionIP.Name = "comboBox_tab4_connetionIP";
            this.comboBox_tab4_connetionIP.Size = new System.Drawing.Size(121, 21);
            this.comboBox_tab4_connetionIP.TabIndex = 18;
            this.comboBox_tab4_connetionIP.Text = "127.0.0.1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(1136, 583);
            this.Controls.Add(this.tabControl_main);
            this.Name = "Form1";
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.panel_tab1_main.ResumeLayout(false);
            this.panel_tab1_main.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_tab1_Tree)).EndInit();
            this.tabControl_main.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ComboBox comboBox_tab2_QueryNumber;
        private System.Windows.Forms.Button btn_tab2_SelectQuery;
        private System.Windows.Forms.TextBox textBox_tab2_SelectResult;
        private System.Windows.Forms.TextBox textBox_tab2_Query;
        private System.Windows.Forms.Button btn_tab2_CreateSelect;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Panel panel_tab1_main;
        private System.Windows.Forms.ComboBox comboBox_tab1_QueryNumber;
        private System.Windows.Forms.Button btn_tab1_Debug;
        private System.Windows.Forms.Button btn_tab1_SelectQuerry;
        private System.Windows.Forms.Button btn_tab1_SaveTree;
        private System.Windows.Forms.PictureBox pictureBox_tab1_Tree;
        private System.Windows.Forms.Button btn_tab1_CreateTree;
        private System.Windows.Forms.TextBox textBox_tab1_Query;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.Button btn_tab2_CreateJoin;
        private System.Windows.Forms.TextBox textBox_tab2_JoinResult;
        private System.Windows.Forms.TextBox textBox_tab2_SortResult;
        private System.Windows.Forms.Button btn_tab2_CreateSort;
        private System.Windows.Forms.Button btn_tab2_CreateAll;
        private System.Windows.Forms.TextBox textBox_tab2_AllResult;
        private System.Windows.Forms.CheckBox checkBox_tab1_DisableHeavyQuerry;
        private System.Windows.Forms.RichTextBox richTextBox_tab1_Query;
        private System.Windows.Forms.CheckBox checkBox_tab2_DisableHeavyQuerry;
        private System.Windows.Forms.CheckBox checkBox_Tab2_ClusterXNEnable;
        private System.Windows.Forms.ComboBox comboBox_tab2_IP;
        private System.Windows.Forms.CheckBox checkBox_Tab2_ClusterixN_Online;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.ComboBox comboBox_tab4_connetionIP;
        private System.Windows.Forms.Button btn_tab4_SendToClusterix;
        private System.Windows.Forms.RichTextBox richTextBox_tab4_XML;
    }
}

