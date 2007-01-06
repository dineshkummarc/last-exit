INCLUDES = 						\
	-I$(top_srcdir)					\
	-DG_LOG_DOMAIN=\"liblast-exit\"			\
	-DGNOMELOCALEDIR=\""$(datadir)/locale"\"	\
	$(LASTEXIT_CFLAGS)				\
	-DG_DISABLE_DEPRECATED

lastexitlibdir = $(pkglibdir)
lastexitlib_LTLIBRARIES = liblastexit.la

liblastexit_la_SOURCES = 				\
	player.c					\
	player.h					\
	sexy-icon-entry.c				\
	sexy-icon-entry.h				\
	sexy-url-label.c				\
	sexy-url-label.h

liblastexit_la_LIBADD = $(LASTEXIT_LIBS)
liblastexit_la_LDFLAGS = -avoid-version 
