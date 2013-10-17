@Echo Off

SET PROGRAMS=\\its-lpapp1\lifepro\NPSET16\start
SET PATH=\\its-lpapp1\lifepro\NPSET16\start;\\its-lpapp1\lifepro\NPSET16\blic;\\its-lpapp1\lifepro\NPSET16\exec;%PATH%
SET IMAGE=C\\its-lpapp1\lifepro\NPSET16\image
SET WORKAREA=\\its-lpapp1\lifepro\NPSET16\workarea
SET LAUNCHPATH=\\its-lpapp1\lifepro\NPSET16\start
SET XAMLPATH=\\its-lpapp1\Lifepro\Npset16\BlicRC;\\its-lpapp1\lifepro\NPSET16\rc
SET DATASOURCETYPE=S
SET @ODBC_INF=\\its-lpapp1\lifepro\NPSET16\start\NPSET16.inf
SET @CBR_TRAILING_BLANK_RECORD=REMOVE
SET @NOMESSAGE=YES
SET LPCONNSTRING=data source=ITS-LPDB1\LPP1;initial catalog=NPSET16;user id=PROD16_admin;password=Pdma1201
SET LPCTRLCONNSTRING=data source=ITS-LPDB1\LPP1;initial catalog=NPSET16;user id=PROD16_admin;password=Pdma1201
SET SQL_CTRL_DATASRC=NPSET16
SET SQL_DATASRC=NPSET16
SET LP_MCACHE_SIZE=50000
SET LP_LOG_OPTION=Y
SET ES-CODER=%1
SET PRUNPROG=BPSPCLOS

\\its-lpapp1\lifepro\NPSET16\start\RUNFJ.EXE BLPINVOK
