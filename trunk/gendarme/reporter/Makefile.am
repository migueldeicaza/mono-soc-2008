reporterdir=
reporter_SCRIPTS = reporter.exe 
reporter_DATA = 
EXTRA_DIST = 
CLEANFILES = 
DISTCLEANFILES = Makefile.in 

reporter_sources =  \
	MasterPageResultWriter.cs \
	ReporterLauncher.cs

reporter_build_sources = $(addprefix $(srcdir)/, $(reporter_sources))


reporter.exe: $(reporter_build_sources) $(reporter_SCRIPTS)  
	$(GMCS) -target:exe -pkg:mono-cecil -pkg:gendarme-framework \
		-r:/home/nestor/opt/mono/lib/gendarme/gendarme.exe \
		-out:$@ $(reporter_build_sources)

