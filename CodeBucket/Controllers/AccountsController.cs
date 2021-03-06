using System;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using CodeBucket.Data;
using CodeBucket.Elements;

namespace CodeBucket.Controllers
{
	/// <summary>
	/// A list of the accounts that are currently listed with the application
	/// </summary>
	public class AccountsController : BaseDialogViewController
	{
		/// <summary>
		/// Occurs when account selected.
		/// </summary>
		public event Action<Account> AccountSelected;

		/// <summary>
		/// Raises the account selected event.
		/// </summary>
		/// <param name="account">Account.</param>
		protected virtual void OnAccountSelected(Account account)
		{
			var d = AccountSelected;
			if (d != null)
				d(account);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AccountsController"/> class.
		/// </summary>
		public AccountsController ()
			: base(true, "Accounts")
		{
			Title = "Accounts";
			Style = UITableViewStyle.Grouped;
		}

		/// <summary>
		/// Views the will appear.
		/// </summary>
		/// <param name="animated">If set to <c>true</c> animated.</param>
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			
			var accountSection = new Section();
			foreach (var account in Application.Accounts)
			{
				var thisAccount = account;
				var t = new StyledElement(thisAccount.Username, thisAccount.AccountType.ToString(), UITableViewCellStyle.Subtitle) { Image = Images.Anonymous };
				if (!string.IsNullOrEmpty(thisAccount.AvatarUrl))
					t.ImageUri = new Uri(thisAccount.AvatarUrl);
				
				t.Tapped += () => { 
					if (thisAccount.DontRemember)
					{
						if (thisAccount.AccountType == Account.Type.Bitbucket)
						{
							var loginController = new CodeBucket.Bitbucket.Controllers.Accounts.LoginViewController { Username = thisAccount.Username };
							loginController.LoginComplete = a => {
								NavigationController.PopToViewController(this, true);
								OnAccountSelected(a); 
							};
							NavigationController.PushViewController(loginController, true);
						}
					}
					else
					{
						OnAccountSelected(thisAccount);
					}
				};
				
				accountSection.Add(t);
			}

			var addSection = new Section();
			var addAccount = new StyledElement("Add Account", () => {
                var ctrl = new Bitbucket.Controllers.Accounts.LoginViewController();
                ctrl.LoginComplete = (a) => NavigationController.PopToViewController(this, true);
				NavigationController.PushViewController(ctrl, true);
			});
			//addAccount.Image = Images.CommentAdd;
			addSection.Add(addAccount);

			Root = new RootElement(Title) { accountSection, addSection };
		}
	}
}

