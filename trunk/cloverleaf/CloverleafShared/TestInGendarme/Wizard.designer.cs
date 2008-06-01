namespace CloverleafShared.TestInGendarme
{

	partial class Wizard {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose (bool disposing)
		{
			if (disposing && (components != null)) {
				components.Dispose ();
			}
			base.Dispose (disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent ()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Wizard));
            this.wizard_tab_control = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.projects_select_all_button = new System.Windows.Forms.Button();
            this.projects_clear_all_button = new System.Windows.Forms.Button();
            this.project_list_box = new System.Windows.Forms.CheckedListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.welcome_title_label = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.addfiles_tab_page = new System.Windows.Forms.TabPage();
            this.addfiles_select_all_button = new System.Windows.Forms.Button();
            this.addfiles_clear_all_button = new System.Windows.Forms.Button();
            this.file_list_box = new System.Windows.Forms.CheckedListBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.browse_documentation_button = new System.Windows.Forms.Button();
            this.rules_count_label = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.rules_tree_view = new System.Windows.Forms.TreeView();
            this.analyze_tab_tage = new System.Windows.Forms.TabPage();
            this.analyze_defect_label = new System.Windows.Forms.Label();
            this.analyze_title_label = new System.Windows.Forms.Label();
            this.analyze_status_label = new System.Windows.Forms.Label();
            this.analyze_assembly_label = new System.Windows.Forms.Label();
            this.progress_bar = new System.Windows.Forms.ProgressBar();
            this.results_tab_Page = new System.Windows.Forms.TabPage();
            this.view_report_button = new System.Windows.Forms.Button();
            this.save_report_button = new System.Windows.Forms.Button();
            this.results_title_label = new System.Windows.Forms.Label();
            this.report_subtitle_label = new System.Windows.Forms.Label();
            this.help_button = new System.Windows.Forms.Button();
            this.cancel_button = new System.Windows.Forms.Button();
            this.next_button = new System.Windows.Forms.Button();
            this.back_button = new System.Windows.Forms.Button();
            this.open_file_dialog = new System.Windows.Forms.OpenFileDialog();
            this.save_file_dialog = new System.Windows.Forms.SaveFileDialog();
            this.wizard_tab_control.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.addfiles_tab_page.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.analyze_tab_tage.SuspendLayout();
            this.results_tab_Page.SuspendLayout();
            this.SuspendLayout();
            // 
            // wizard_tab_control
            // 
            this.wizard_tab_control.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.wizard_tab_control.Controls.Add(this.tabPage1);
            this.wizard_tab_control.Controls.Add(this.addfiles_tab_page);
            this.wizard_tab_control.Controls.Add(this.tabPage3);
            this.wizard_tab_control.Controls.Add(this.analyze_tab_tage);
            this.wizard_tab_control.Controls.Add(this.results_tab_Page);
            this.wizard_tab_control.Location = new System.Drawing.Point(12, 22);
            this.wizard_tab_control.Name = "wizard_tab_control";
            this.wizard_tab_control.SelectedIndex = 0;
            this.wizard_tab_control.Size = new System.Drawing.Size(622, 415);
            this.wizard_tab_control.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.projects_select_all_button);
            this.tabPage1.Controls.Add(this.projects_clear_all_button);
            this.tabPage1.Controls.Add(this.project_list_box);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.welcome_title_label);
            this.tabPage1.Controls.Add(this.label10);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(614, 386);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Welcome";
            // 
            // projects_select_all_button
            // 
            this.projects_select_all_button.Location = new System.Drawing.Point(298, 351);
            this.projects_select_all_button.Name = "projects_select_all_button";
            this.projects_select_all_button.Size = new System.Drawing.Size(150, 23);
            this.projects_select_all_button.TabIndex = 13;
            this.projects_select_all_button.Text = "Select All";
            this.projects_select_all_button.UseVisualStyleBackColor = true;
            this.projects_select_all_button.Click += new System.EventHandler(this.ProjectsSelectAllClick);
            // 
            // projects_clear_all_button
            // 
            this.projects_clear_all_button.Location = new System.Drawing.Point(454, 351);
            this.projects_clear_all_button.Name = "projects_clear_all_button";
            this.projects_clear_all_button.Size = new System.Drawing.Size(150, 23);
            this.projects_clear_all_button.TabIndex = 12;
            this.projects_clear_all_button.Text = "Clear All";
            this.projects_clear_all_button.UseVisualStyleBackColor = true;
            this.projects_clear_all_button.Click += new System.EventHandler(this.ProjectsClearAllClick);
            // 
            // project_list_box
            // 
            this.project_list_box.CheckOnClick = true;
            this.project_list_box.FormattingEnabled = true;
            this.project_list_box.Location = new System.Drawing.Point(18, 146);
            this.project_list_box.Name = "project_list_box";
            this.project_list_box.Size = new System.Drawing.Size(586, 199);
            this.project_list_box.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(307, 117);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Cloverleaf";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(307, 130);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(245, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Copyright © 2008 Edward Ropple and contributors";
            // 
            // welcome_title_label
            // 
            this.welcome_title_label.AutoSize = true;
            this.welcome_title_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.welcome_title_label.Location = new System.Drawing.Point(15, 12);
            this.welcome_title_label.Name = "welcome_title_label";
            this.welcome_title_label.Size = new System.Drawing.Size(170, 13);
            this.welcome_title_label.TabIndex = 8;
            this.welcome_title_label.Text = "Cloverleaf Gendarme Runner";
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(15, 27);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(589, 81);
            this.label10.TabIndex = 5;
            this.label10.Text = resources.GetString("label10.Text");
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 117);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Gendarme";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 130);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(253, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Copyright © 2005-2008 Novell, Inc. and contributors";
            // 
            // addfiles_tab_page
            // 
            this.addfiles_tab_page.Controls.Add(this.addfiles_select_all_button);
            this.addfiles_tab_page.Controls.Add(this.addfiles_clear_all_button);
            this.addfiles_tab_page.Controls.Add(this.file_list_box);
            this.addfiles_tab_page.Controls.Add(this.label6);
            this.addfiles_tab_page.Controls.Add(this.label5);
            this.addfiles_tab_page.Location = new System.Drawing.Point(4, 25);
            this.addfiles_tab_page.Name = "addfiles_tab_page";
            this.addfiles_tab_page.Padding = new System.Windows.Forms.Padding(3);
            this.addfiles_tab_page.Size = new System.Drawing.Size(614, 386);
            this.addfiles_tab_page.TabIndex = 1;
            this.addfiles_tab_page.Text = "Add Files";
            this.addfiles_tab_page.UseVisualStyleBackColor = true;
            // 
            // addfiles_select_all_button
            // 
            this.addfiles_select_all_button.Location = new System.Drawing.Point(298, 352);
            this.addfiles_select_all_button.Name = "addfiles_select_all_button";
            this.addfiles_select_all_button.Size = new System.Drawing.Size(150, 23);
            this.addfiles_select_all_button.TabIndex = 15;
            this.addfiles_select_all_button.Text = "Select All";
            this.addfiles_select_all_button.UseVisualStyleBackColor = true;
            this.addfiles_select_all_button.Click += new System.EventHandler(AssembliesSelectAllClick);
            // 
            // addfiles_clear_all_button
            // 
            this.addfiles_clear_all_button.Location = new System.Drawing.Point(454, 352);
            this.addfiles_clear_all_button.Name = "addfiles_clear_all_button";
            this.addfiles_clear_all_button.Size = new System.Drawing.Size(150, 23);
            this.addfiles_clear_all_button.TabIndex = 14;
            this.addfiles_clear_all_button.Text = "Clear All";
            this.addfiles_clear_all_button.UseVisualStyleBackColor = true;
            this.addfiles_clear_all_button.Click += new System.EventHandler(AssembliesClearAllClick);
            // 
            // file_list_box
            // 
            this.file_list_box.FormattingEnabled = true;
            this.file_list_box.Location = new System.Drawing.Point(18, 102);
            this.file_list_box.Name = "file_list_box";
            this.file_list_box.Size = new System.Drawing.Size(586, 244);
            this.file_list_box.TabIndex = 10;
            System.Windows.Forms.CheckedListBox.CheckForIllegalCrossThreadCalls = false;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(15, 25);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(589, 54);
            this.label6.TabIndex = 8;
            this.label6.Text = resources.GetString("label6.Text");
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(15, 12);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(115, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Add Assembly Files";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.browse_documentation_button);
            this.tabPage3.Controls.Add(this.rules_count_label);
            this.tabPage3.Controls.Add(this.label8);
            this.tabPage3.Controls.Add(this.label7);
            this.tabPage3.Controls.Add(this.rules_tree_view);
            this.tabPage3.Location = new System.Drawing.Point(4, 25);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(614, 386);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Select Rules";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // browse_documentation_button
            // 
            this.browse_documentation_button.Location = new System.Drawing.Point(327, 354);
            this.browse_documentation_button.Name = "browse_documentation_button";
            this.browse_documentation_button.Size = new System.Drawing.Size(156, 23);
            this.browse_documentation_button.TabIndex = 5;
            this.browse_documentation_button.Text = "Browse Documentation...";
            this.browse_documentation_button.UseVisualStyleBackColor = true;
            this.browse_documentation_button.Click += new System.EventHandler(this.BrowseDocumentationButtonClick);
            // 
            // rules_count_label
            // 
            this.rules_count_label.AutoSize = true;
            this.rules_count_label.Location = new System.Drawing.Point(15, 351);
            this.rules_count_label.Name = "rules_count_label";
            this.rules_count_label.Size = new System.Drawing.Size(112, 13);
            this.rules_count_label.TabIndex = 10;
            this.rules_count_label.Text = "{0} rules are available.";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(28, 26);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(308, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = "Select the rules to be executed against the specified assemblies";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(15, 12);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(79, 13);
            this.label7.TabIndex = 8;
            this.label7.Text = "Select Rules";
            // 
            // rules_tree_view
            // 
            this.rules_tree_view.CheckBoxes = true;
            this.rules_tree_view.HideSelection = false;
            this.rules_tree_view.HotTracking = true;
            this.rules_tree_view.Location = new System.Drawing.Point(15, 47);
            this.rules_tree_view.Name = "rules_tree_view";
            this.rules_tree_view.ShowNodeToolTips = true;
            this.rules_tree_view.Size = new System.Drawing.Size(468, 301);
            this.rules_tree_view.TabIndex = 0;
            this.rules_tree_view.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.RulesTreeViewAfterCheck);
            // 
            // analyze_tab_tage
            // 
            this.analyze_tab_tage.Controls.Add(this.analyze_defect_label);
            this.analyze_tab_tage.Controls.Add(this.analyze_title_label);
            this.analyze_tab_tage.Controls.Add(this.analyze_status_label);
            this.analyze_tab_tage.Controls.Add(this.analyze_assembly_label);
            this.analyze_tab_tage.Controls.Add(this.progress_bar);
            this.analyze_tab_tage.Location = new System.Drawing.Point(4, 25);
            this.analyze_tab_tage.Name = "analyze_tab_tage";
            this.analyze_tab_tage.Size = new System.Drawing.Size(614, 386);
            this.analyze_tab_tage.TabIndex = 3;
            this.analyze_tab_tage.Text = "Analyze";
            this.analyze_tab_tage.UseVisualStyleBackColor = true;
            // 
            // analyze_defect_label
            // 
            this.analyze_defect_label.AutoSize = true;
            this.analyze_defect_label.Location = new System.Drawing.Point(15, 105);
            this.analyze_defect_label.Name = "analyze_defect_label";
            this.analyze_defect_label.Size = new System.Drawing.Size(89, 13);
            this.analyze_defect_label.TabIndex = 10;
            this.analyze_defect_label.Text = "Defects Found: 0";
            // 
            // analyze_title_label
            // 
            this.analyze_title_label.AutoSize = true;
            this.analyze_title_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.analyze_title_label.Location = new System.Drawing.Point(15, 12);
            this.analyze_title_label.Name = "analyze_title_label";
            this.analyze_title_label.Size = new System.Drawing.Size(131, 13);
            this.analyze_title_label.TabIndex = 9;
            this.analyze_title_label.Text = "Analysis in progress...";
            // 
            // analyze_status_label
            // 
            this.analyze_status_label.AutoSize = true;
            this.analyze_status_label.Location = new System.Drawing.Point(15, 57);
            this.analyze_status_label.Name = "analyze_status_label";
            this.analyze_status_label.Size = new System.Drawing.Size(135, 13);
            this.analyze_status_label.TabIndex = 2;
            this.analyze_status_label.Text = "Processing assembly 0 of 1";
            // 
            // analyze_assembly_label
            // 
            this.analyze_assembly_label.AutoSize = true;
            this.analyze_assembly_label.Location = new System.Drawing.Point(15, 80);
            this.analyze_assembly_label.Name = "analyze_assembly_label";
            this.analyze_assembly_label.Size = new System.Drawing.Size(57, 13);
            this.analyze_assembly_label.TabIndex = 1;
            this.analyze_assembly_label.Text = "Assembly: ";
            // 
            // progress_bar
            // 
            this.progress_bar.Location = new System.Drawing.Point(18, 133);
            this.progress_bar.Name = "progress_bar";
            this.progress_bar.Size = new System.Drawing.Size(465, 23);
            this.progress_bar.TabIndex = 0;
            // 
            // results_tab_Page
            // 
            this.results_tab_Page.Controls.Add(this.view_report_button);
            this.results_tab_Page.Controls.Add(this.save_report_button);
            this.results_tab_Page.Controls.Add(this.results_title_label);
            this.results_tab_Page.Controls.Add(this.report_subtitle_label);
            this.results_tab_Page.Location = new System.Drawing.Point(4, 25);
            this.results_tab_Page.Name = "results_tab_Page";
            this.results_tab_Page.Size = new System.Drawing.Size(614, 386);
            this.results_tab_Page.TabIndex = 4;
            this.results_tab_Page.Text = "Results";
            this.results_tab_Page.UseVisualStyleBackColor = true;
            // 
            // view_report_button
            // 
            this.view_report_button.Location = new System.Drawing.Point(15, 126);
            this.view_report_button.Name = "view_report_button";
            this.view_report_button.Size = new System.Drawing.Size(142, 23);
            this.view_report_button.TabIndex = 11;
            this.view_report_button.Text = "View Report...";
            this.view_report_button.UseVisualStyleBackColor = true;
            this.view_report_button.Click += new System.EventHandler(this.ViewReportButtonClick);
            // 
            // save_report_button
            // 
            this.save_report_button.Location = new System.Drawing.Point(15, 97);
            this.save_report_button.Name = "save_report_button";
            this.save_report_button.Size = new System.Drawing.Size(142, 23);
            this.save_report_button.TabIndex = 10;
            this.save_report_button.Text = "Save Report...";
            this.save_report_button.UseVisualStyleBackColor = true;
            this.save_report_button.Click += new System.EventHandler(this.SaveReportButtonClick);
            // 
            // results_title_label
            // 
            this.results_title_label.AutoSize = true;
            this.results_title_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.results_title_label.Location = new System.Drawing.Point(15, 12);
            this.results_title_label.Name = "results_title_label";
            this.results_title_label.Size = new System.Drawing.Size(99, 13);
            this.results_title_label.TabIndex = 9;
            this.results_title_label.Text = "Analysis Results";
            // 
            // report_subtitle_label
            // 
            this.report_subtitle_label.AutoSize = true;
            this.report_subtitle_label.Location = new System.Drawing.Point(28, 26);
            this.report_subtitle_label.Name = "report_subtitle_label";
            this.report_subtitle_label.Size = new System.Drawing.Size(236, 13);
            this.report_subtitle_label.TabIndex = 0;
            this.report_subtitle_label.Text = "Gendarme has found {0} defects during analysis.";
            // 
            // help_button
            // 
            this.help_button.Location = new System.Drawing.Point(545, 411);
            this.help_button.Name = "help_button";
            this.help_button.Size = new System.Drawing.Size(75, 23);
            this.help_button.TabIndex = 4;
            this.help_button.Text = "Help";
            this.help_button.UseVisualStyleBackColor = true;
            this.help_button.Click += new System.EventHandler(this.HelpButtonClick);
            // 
            // cancel_button
            // 
            this.cancel_button.Location = new System.Drawing.Point(464, 411);
            this.cancel_button.Name = "cancel_button";
            this.cancel_button.Size = new System.Drawing.Size(75, 23);
            this.cancel_button.TabIndex = 3;
            this.cancel_button.Text = "Cancel";
            this.cancel_button.UseVisualStyleBackColor = true;
            this.cancel_button.Click += new System.EventHandler(this.CancelButtonClick);
            // 
            // next_button
            // 
            this.next_button.Location = new System.Drawing.Point(383, 411);
            this.next_button.Name = "next_button";
            this.next_button.Size = new System.Drawing.Size(75, 23);
            this.next_button.TabIndex = 2;
            this.next_button.Text = "Next >";
            this.next_button.UseVisualStyleBackColor = true;
            this.next_button.Click += new System.EventHandler(this.NextButtonClick);
            // 
            // back_button
            // 
            this.back_button.Location = new System.Drawing.Point(302, 411);
            this.back_button.Name = "back_button";
            this.back_button.Size = new System.Drawing.Size(75, 23);
            this.back_button.TabIndex = 1;
            this.back_button.Text = "< Back";
            this.back_button.UseVisualStyleBackColor = true;
            this.back_button.Click += new System.EventHandler(this.BackButtonClick);
            // 
            // open_file_dialog
            // 
            this.open_file_dialog.Filter = "Assemblies (*.exe;*.dll)|*.exe;*.dll|All files (*.*)|*.*";
            this.open_file_dialog.Multiselect = true;
            this.open_file_dialog.Title = "Add Assemblies...";
            // 
            // save_file_dialog
            // 
            this.save_file_dialog.DefaultExt = "*.html";
            this.save_file_dialog.Filter = "HTML report (*.html)|*.html|XML report (*.xml)|*.xml|Text report (*.txt)|*.txt";
            this.save_file_dialog.Title = "Save Report To...";
            // 
            // Wizard
            // 
            this.AcceptButton = this.next_button;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 455);
            this.ControlBox = false;
            this.Controls.Add(this.back_button);
            this.Controls.Add(this.next_button);
            this.Controls.Add(this.cancel_button);
            this.Controls.Add(this.help_button);
            this.Controls.Add(this.wizard_tab_control);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(640, 480);
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "Wizard";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Gendarme";
            this.wizard_tab_control.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.addfiles_tab_page.ResumeLayout(false);
            this.addfiles_tab_page.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.analyze_tab_tage.ResumeLayout(false);
            this.analyze_tab_tage.PerformLayout();
            this.results_tab_Page.ResumeLayout(false);
            this.results_tab_Page.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl wizard_tab_control;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage addfiles_tab_page;
		private System.Windows.Forms.Button help_button;
        private System.Windows.Forms.Button cancel_button;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.TreeView rules_tree_view;
		private System.Windows.Forms.TabPage analyze_tab_tage;
		private System.Windows.Forms.TabPage results_tab_Page;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button next_button;
        private System.Windows.Forms.Button back_button;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.OpenFileDialog open_file_dialog;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label rules_count_label;
		private System.Windows.Forms.Button browse_documentation_button;
		private System.Windows.Forms.Label report_subtitle_label;
		private System.Windows.Forms.SaveFileDialog save_file_dialog;
        private System.Windows.Forms.Label welcome_title_label;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Button view_report_button;
		private System.Windows.Forms.Button save_report_button;
        private System.Windows.Forms.Label results_title_label;
        private System.Windows.Forms.Label analyze_title_label;
		private System.Windows.Forms.Label analyze_assembly_label;
		private System.Windows.Forms.ProgressBar progress_bar;
		private System.Windows.Forms.Label analyze_status_label;
		private System.Windows.Forms.Label analyze_defect_label;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckedListBox project_list_box;
        private System.Windows.Forms.Button projects_clear_all_button;
        private System.Windows.Forms.Button projects_select_all_button;
        private System.Windows.Forms.CheckedListBox file_list_box;
        private System.Windows.Forms.Button addfiles_select_all_button;
        private System.Windows.Forms.Button addfiles_clear_all_button;
	}
}

