using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pandora.UI.Login
{
    public class LoginButton : MonoBehaviour
    {
        public Text UsernameText = null;
        public Text PasswordText = null;

        public void Login()
        {
            if (UsernameText == null || PasswordText == null) return;
            
            var username = UsernameText.text;
            var password = PasswordText.text;


        }
    }
}