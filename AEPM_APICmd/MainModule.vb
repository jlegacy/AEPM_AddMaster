Module MainModule

    Dim parmSweep As String
    Dim parmSession As String
    Dim parmEnv As String

    Dim mFunction As String
    Dim mReqPartNo As String
    Dim mReqPrimeProdClass As String

    Dim WebServ As New AEPM_Services.Services

    Dim AS400Conn As New OleDb.OleDbConnection
    Dim AS400Cmd As New OleDb.OleDbCommand

    Dim AS400Conn2 As New OleDb.OleDbConnection
    Dim AS400Cmd2 As New OleDb.OleDbCommand

    Dim AS400Conn3 As New OleDb.OleDbConnection
    Dim AS400Cmd3 As New OleDb.OleDbCommand

    Dim AS400Conn4 As New OleDb.OleDbConnection
    Dim AS400Cmd4 As New OleDb.OleDbCommand

    Dim AS400Conn5 As New OleDb.OleDbConnection
    Dim AS400Cmd5 As New OleDb.OleDbCommand

    Sub Main()
        Dim parmString() As String
        parmString = System.Environment.GetCommandLineArgs
        parmSweep = parmString(1)

        If (parmSweep.CompareTo("process") = 0) Then
            AS400Conn.ConnectionString = "Provider=IBMDASQL;Data Source=" & My.Settings.as400 & ";User Id=SQLUSER;Password=PDC2051"
            AS400Conn2.ConnectionString = "Provider=IBMDASQL;Data Source=" & My.Settings.as400 & ";User Id=SQLUSER;Password=PDC2051"
            AS400Conn3.ConnectionString = "Provider=IBMDASQL;Data Source=" & My.Settings.as400 & ";User Id=SQLUSER;Password=PDC2051"
            AS400Conn4.ConnectionString = "Provider=IBMDASQL;Data Source=" & My.Settings.as400 & ";User Id=SQLUSER;Password=PDC2051"
            AS400Conn5.ConnectionString = "Provider=IBMDASQL;Data Source=" & My.Settings.as400 & ";User Id=SQLUSER;Password=PDC2051"

            AS400Cmd.CommandType = CommandType.Text

            AS400Conn.Open()
            AS400Cmd.Connection = AS400Conn

            AS400Conn2.Open()
            AS400Cmd2.Connection = AS400Conn2

            AS400Conn3.Open()
            AS400Cmd3.Connection = AS400Conn3

            AS400Conn4.Open()
            AS400Cmd4.Connection = AS400Conn4

            AS400Conn5.Open()
            AS400Cmd5.Connection = AS400Conn5

            ReadAS400BufferTable()

            AS400Conn.Close()
            AS400Conn2.Close()
            AS400Conn3.Close()
            AS400Conn4.Close()
            AS400Conn5.Close()
        End If

        If (parmSweep.CompareTo("sweep") = 0) Then
            sendSweepfiles()
        End If

    End Sub
    Sub sendSweepfiles()
        Dim attachment As New List(Of String)
        Dim fileName As String

        Dim successFileNameBackup As String
        Dim errorFileNameBackup As String

        Dim ID1 As String = DateValue(Now).Year & DateValue(Now).Month & DateValue(Now).Day _
                    & TimeValue(Now).Hour & TimeValue(Now).Minute & TimeValue(Now).Second _
                    & Guid.NewGuid().ToString()
      
        successFileNameBackup = "S-" & ID1 & ".csv"
        errorFileNameBackup = "E-" & ID1 & ".csv"

        'send successess'
        fileName = My.Application.Info.DirectoryPath & "\" & My.Settings.successFile
        If My.Computer.FileSystem.FileExists(fileName) Then
            My.Computer.FileSystem.RenameFile(fileName, successFileNameBackup)
            fileName = My.Application.Info.DirectoryPath & "\" & successFileNameBackup
            attachment.Add(fileName)
        End If

        'send failures'
        fileName = My.Application.Info.DirectoryPath & "\" & My.Settings.errorFile
        If My.Computer.FileSystem.FileExists(fileName) Then
            My.Computer.FileSystem.RenameFile(fileName, errorFileNameBackup)
            fileName = My.Application.Info.DirectoryPath & "\" & errorFileNameBackup
            attachment.Add(fileName)
        End If

        If attachment.Count > 0 Then
            Mailer.SendMail(From:=My.Settings.fromEmail, _
                     To:=My.Settings.toEmail, _
                     Subject:="AEPM Results for: " & DateValue(Now), _
                     Body:="See attached AEPM Results for " & DateValue(Now), _
                     Attachments:=attachment, _
                     IsBodyHtml:=False, _
                     MailPort:=25, _
                     MailServer:="aammail02")
        End If

      
    End Sub
    Sub writeSuccessFile(ByVal csvRow As Array)
        Dim csvFile As String = My.Application.Info.DirectoryPath & "\" & My.Settings.successFile
        Dim outFile As IO.StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(csvFile, True)
        Dim outLine As String
        Dim fld As Object
        Dim count As Integer
        Dim dtmTest As Date

        outLine = ""
        count = 0

        For Each fld In csvRow
            count = count + 1
            If count = 1 Then
                dtmTest = DateValue(Now)
                outLine = dtmTest & "," & fld.PartNumber & "," & fld.Brand
            Else
                outLine = outLine & "," & fld.PartNumber & "," & fld.Brand
            End If
        Next fld

        outFile.WriteLine(outLine)
        outFile.Close()

    End Sub
    Sub writeErrorFile(ByVal part As String, ByVal errorMsg As String)
        Dim csvFile As String = My.Application.Info.DirectoryPath & "\" & My.Settings.errorFile
        Dim outFile As IO.StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(csvFile, True)
        Dim outLine As String
        Dim dtmTest As Date

        outLine = ""

        dtmTest = DateValue(Now)
        outLine = dtmTest & "," & part & "," & errorMsg
      
        outFile.WriteLine(outLine)
        outFile.Close()

    End Sub

    Class Mailer
        ''one static method for sending e-mails
        Shared Sub SendMail(ByVal [From] As String, ByVal [To] As String, _
                            ByVal Subject As String, ByVal Body As String, ByVal MailServer _
                            As String, Optional ByVal IsBodyHtml As Boolean = True, _
                            Optional ByVal MailPort As Integer = 25, _
                            Optional ByVal Attachments As List(Of String) = Nothing, Optional _
                            ByVal AuthUsername As String = Nothing, Optional ByVal _
                            AuthPassword As String = Nothing)
            ''create a SmtpClient object to allow applications to send 
            ''e-mail by using the Simple Mail Transfer Protocol (SMTP).
            Dim fileName As Object
            Dim MailClient As System.Net.Mail.SmtpClient = _
            New System.Net.Mail.SmtpClient(MailServer, MailPort)
            ''create a MailMessage object to represent an e-mail message
            ''that can be sent using the SmtpClient class
            Dim MailMessage = New System.Net.Mail.MailMessage( _
            [From], [To], Subject, Body)
            ''sets a value indicating whether the mail message body is in Html.
            MailMessage.IsBodyHtml = IsBodyHtml
            ''sets the credentials used to authenticate the sender
            If (AuthUsername IsNot Nothing) AndAlso (AuthPassword _
                                                     IsNot Nothing) Then
                MailClient.Credentials = New _
                System.Net.NetworkCredential(AuthUsername, AuthPassword)
            End If
            ''add the files as the attachments for the mailmessage object
            If (Attachments IsNot Nothing) Then
                For Each fileName In Attachments
                    MailMessage.Attachments.Add( _
                      New System.Net.Mail.Attachment(fileName))
                Next
            End If
            MailClient.Send(MailMessage)

        End Sub
    End Class

    Sub ReadAS400BufferTable()

        Dim lGetPart As AEPM_Services.GetMasterResult
        Dim lAddPart As AEPM_Services.AddMasterResult

        Dim lPartArray() As AEPM_APICmd.AEPM_Services.CrossPart
        Dim ndx As Integer
        Dim mainPart As String
        Dim recordCount As Int16

        Dim query As String
        Dim query2 As String
        Dim dr As OleDb.OleDbDataReader
        Dim dr2 As OleDb.OleDbDataReader
        Dim dr3 As OleDb.OleDbDataReader
        Dim dr4 As OleDb.OleDbDataReader
        Dim lTime As String
        Dim lTimeN As Long
        Dim partExists As Boolean
        Dim yaleSped As String

        lTime = Now.TimeOfDay.ToString
        lTime = Replace(lTime, ":", "")
        lTimeN = Int(Val(lTime))
        lTimeN = lTimeN - 10000
        If lTimeN < 0 Then lTimeN = lTimeN + 240000

        'pick up all records that are not aging'
        query = "select * from " & My.Settings.library & ".AEPMPRT where AERTYP = 'R' AND AEAERF <> 'A'"

        AS400Cmd.CommandText = query
        dr = AS400Cmd.ExecuteReader
        Do While dr.Read()

            'get part number'
            mReqPartNo = dr("AEPART")
            partExists = False
            lGetPart = WebServ.GetMaster(mReqPartNo)
            If (lGetPart.Success) Then
                partExists = True
            End If

            ' If a Y Sped check for S Sped equivalent, if not,  process record as normal
            ' else have record show as already added
            If partExists = False Then
                If (mReqPartNo.Trim.Length = 12 And (mReqPartNo.Substring(0, 1).CompareTo("Y")) = 0) Then
                    yaleSped = mReqPartNo.Replace("Y", "S")
                    lGetPart = WebServ.GetMaster(yaleSped)
                    If (lGetPart.Success) Then
                        partExists = True
                    End If
                End If
            End If

            'get main driver part based on linked part'
            query = "select AEMAIN from " & My.Settings.library & ".AEXRF where AELINK = '" & mReqPartNo & "'" & " FETCH FIRST 1 ROWS ONLY"
            AS400Cmd2.CommandText = query
            dr2 = AS400Cmd2.ExecuteReader
            If dr2.Read() Then

                If Trim(dr2("AEMAIN")) <> "" Then
                    mainPart = dr2("AEMAIN")
                    query = "select count(*) as recordCount from " & My.Settings.library & ".AEXRF where AEMAIN = '" & mainPart & "'"
                    query2 = "select * from " & My.Settings.library & ".AEXRF where AEMAIN = '" & mainPart & "'"
                    '===============================================================================
                    'get row count for number of queried records'
                    '===============================================================================
                    AS400Cmd4.CommandText = query
                    dr4 = AS400Cmd4.ExecuteReader
                    dr4.Read()
                    recordCount = dr4("recordCount")
                    dr4.Close()
                    '===============================================================================
                    'now query the actual data
                    '===============================================================================
                    AS400Cmd3.CommandText = query2
                    dr3 = AS400Cmd3.ExecuteReader
                    'loop through aexrf and load parts'
                    ReDim lPartArray(recordCount - 1)
                    ndx = 0
                    Do While dr3.Read()
                        lPartArray(ndx) = New AEPM_APICmd.AEPM_Services.CrossPart
                        lPartArray(ndx).Brand = dr3("AELBRD")
                        lPartArray(ndx).PartNumber = dr3("AELINK")
                        ndx = ndx + 1
                    Loop

                    lGetPart.Branded = CBool(dr("AEBRND"))
                    lGetPart.Commodity_Code = CStr(dr("AECMCD"))
                    lGetPart.Level = CInt(dr("AESLVL"))
                    lGetPart.Returnable = CBool(dr("AERTRN"))
                    lGetPart.Status = CStr(dr("AESTAT"))

                    If lGetPart.Level < 1 Or lGetPart.Level > 3 Then lGetPart.Level = 3

                    If Not (partExists) Then

                        lAddPart = WebServ.AddMaster(dr("AEUSER"), lPartArray, lGetPart.Branded, lGetPart.Commodity_Code, lGetPart.Level, lGetPart.Status, lGetPart.Returnable, "", "", 0, 0, "", "") '

                        If lAddPart.Success Then
                            query = "update " & My.Settings.library & ".AEPMPRT set AERTYP = 'P', AEMSG = " & "'Part Added'" & " where (AEPART = '" & dr("AEPART") & "')"
                            writeSuccessFile(lPartArray)
                        Else
                            writeErrorFile(dr("AEPART"), lAddPart.Error)
                            query = "update " & My.Settings.library & ".AEPMPRT set AERTYP = 'P', AEMSG = '" & lAddPart.Error & "' where (AEPART = '" & dr("AEPART") & "')"
                        End If
                    End If

                    If (partExists) Then
                        ' Ignore if part already exists
                        ' WriteErrorFile(dr("AEPART"), "Part Already Exists")
                        query = "update " & My.Settings.library & ".AEPMPRT set AERTYP = 'P', AEMSG = " & "'Part Already Exists'" & " where (AEPART = '" & dr("AEPART") & "')"
                    End If

                    AS400Cmd5.CommandText = query
                    AS400Cmd5.ExecuteNonQuery()

                    dr3.Close()
                End If
            End If

            dr2.Close()
        Loop

        dr.Close()

    End Sub

End Module
