namespace Infrastructure.Email.Templates
{
    public static class PasswordResetTemplate
    {
        public static string BuildPasswordResetEmailBody(string resetCode, string recipientEmail)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Goodie Habbi - Password Reset</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
            -webkit-text-size-adjust: none;
            width: 100% !important;
        }}
        .container {{
            max-width: 600px;
            margin: 20px auto;
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.05);
            overflow: hidden;
        }}
        .header {{
            background-color: #1987EE; /* Primary brand color */
            color: #ffffff;
            padding: 20px 30px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 26px;
            font-weight: 600;
        }}
        .content {{
            padding: 30px;
            line-height: 1.6;
            color: #4b465d; /* Secondary brand color */
            font-size: 15px;
        }}
        .content p {{
            margin-bottom: 15px;
        }}
        .code-box {{
            background-color: #f2f6fb;
            color: #1987EE;
            font-size: 32px;
            font-weight: bold;
            text-align: center;
            padding: 20px;
            border-radius: 6px;
            margin: 30px 0;
            letter-spacing: 6px;
            border: 1px solid #d9e6f7;
        }}
        .footer {{
            background-color: #f9f9f9;
            color: #777777;
            padding: 20px 30px;
            font-size: 12px;
            text-align: center;
            border-top: 1px solid #dddddd;
        }}
        a {{
            color: #1987EE;
            text-decoration: none;
        }}
        a:hover {{
            text-decoration: underline;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Goodie Habbi</h1>
        </div>
        <div class=""content"">
            <p>Hello {recipientEmail},</p>
            <p>We received a request to reset your password for your Goodie Habbi account. 
            If you did not make this request, please ignore this email.</p>
            <p>Please use the following verification code to reset your password:</p>
            <div class=""code-box"">{resetCode}</div>
            <p>This code is valid for <strong>15 minutes</strong>. 
            Do not share this code with anyone.</p>
            <p>Thank you for using Goodie Habbi!</p>
            <p>Best regards,<br>The Goodie Habbi Team</p>
        </div>
        <div class=""footer"">
            <p>&copy; {DateTime.UtcNow.Year} Goodie Habbi. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

    }
}
