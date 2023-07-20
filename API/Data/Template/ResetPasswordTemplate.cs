namespace API.Data.Template
{
    public static class ResetPasswordTemplate
    {
        public static string ResetPassword(string resetLink, string firstName)
        {
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta http-equiv='X-UA-Compatible' content='IE=edge'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Reset Password</title>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        margin: 0;
                        padding: 0;
                    }}
                    .container {{
                        max-width: 600px;
                        margin: auto;
                        padding: 20px;
                        background-color: #f2f2f2;
                        border-radius: 10px;
                        box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                    }}
                    h2 {{
                        color: #007bff;
                        margin-top: 0;
                    }}
                    p {{
                        margin-bottom: 10px;
                    }}
                    .button {{
                        background-color: #007bff;
                        color: #fff;
                        padding: 12px 24px;
                        text-decoration: none;
                        border-radius: 5px;
                        display: inline-block;
                    }}
                    .button:hover {{
                        background-color: #0056b3;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <h2>Reset Password</h2>
                    <p>Hello {firstName},</p>
                    <p>You have requested to reset your password. Please click the link below to reset your password:</p>
                    <p>
                        <a href='{resetLink}' class='button'>Reset Password</a>
                    </p>
                    <p>If you did not request this, please ignore this email.</p>
                    <p>Best regards,</p>
                    <p>The Connectify Team</p>
                </div>
            </body>
            </html>
        ";
        }
    }
}