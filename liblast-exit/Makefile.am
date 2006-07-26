INCLUDES = 						\
	-I$(top_srcdir)					\
	-DG_LOG_DOMAIN=\"liblast-exit\"			\
	-DGNOMELOCALEDIR=\""$(datadir)/locale"\"	\
	$(LASTEXIT_CFLAGS)				\
	-DG_DISABLE_DEPRECATED

lastexitlibdir = $(pkglibdir)
lastexitlib_LTLIBRARIES = liblastexit.la

DBUS_GENERATED_H = lastexit-dbus-glue.h

lastexit-dbus-glue.h: lastexit.xml
	dbus-binding-tool --mode=glib-server --output=lastexit-dbus-glue.h --prefix=lastexit lastexit.xml

liblastexit_la_SOURCES = 				\
	$(DBUS_GENERATED_H)				\
	lastexit-dbus.c					\
	lastexit-dbus.h					\
	player.c					\
	player.h					\
	sexy-icon-entry.c				\
	sexy-icon-entry.h				\
	sexy-url-label.c				\
	sexy-url-label.h

liblastexit_la_LIBADD = $(LASTEXIT_LIBS)
liblastexit_la_LDFLAGS = -avoid-version 

BUILT_SOURCES = $(DBUS_GENERATED)
CLEANFILES = $(DBUS_GENERATED)
