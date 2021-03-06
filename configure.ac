AC_INIT(last-exit, 6, iain@gnome.org)
AC_PREREQ(2.50)
AC_CONFIG_SRCDIR(src/Driver.cs)
AC_COPYRIGHT([Copyright 2005-2007 Iain Holmes])

AM_CONFIG_HEADER(config.h)

AM_INIT_AUTOMAKE([1.9 foreign dist-bzip2 no-dist-gzip])

AM_MAINTAINER_MODE

AC_PROG_INTLTOOL([0.21])

AC_PROG_CC
AC_ISC_POSIX
AC_HEADER_STDC
AC_CHECK_FUNCS(memmem)
AM_PROG_LIBTOOL

GSTREAMER_REQUIRED=0.10.11
GTK_REQUIRED=2.6
MONO_REQUIRED=1.0
GTKSHARP_REQUIRED=1.9.2

PKG_CHECK_MODULES(LASTEXIT,
		  gconf-2.0 \
		  gdk-pixbuf-2.0 \
		  gtk+-2.0 >= $GTK_REQUIRED \
		  gstreamer-0.10 >= $GSTREAMER_REQUIRED \
		  gstreamer-base-0.10			\
		  gstreamer-plugins-base-0.10 >= $GSTREAMER_REQUIRED \
		  libsexy >= 0.1.5 \
		  dbus-1 >= 0.60 \
		  dbus-glib-1 >= 0.60)
AC_SUBST(LASTEXIT_CFLAGS)
AC_SUBST(LASTEXIT_LIBS)

PKG_CHECK_MODULES(GTKSHARP,
		  gtk-sharp-2.0 >= $GTKSHARP_REQUIRED \
		  gnome-sharp-2.0 >= $GTKSHARP_REQUIRED \
		  glade-sharp-2.0 >= $GTKSHARP_REQUIRED \
		  gconf-sharp-2.0 >= $GTKSHARP_REQUIRED)

AC_PATH_PROG(GCONFTOOL, gconftool-2, no)
AM_GCONF_SOURCE_2

PKG_CHECK_MODULES(DBUS, ndesk-dbus-1.0 >= 0.4 \
	ndesk-dbus-glib-1.0 >= 0.3, have_external_dbus="yes", have_external_dbus="no")
if test "x$have_external_dbus" != "xyes"; then
	AC_MSG_ERROR([dbus-sharp >= 0.4 not found])
fi
AM_CONDITIONAL(EXTERNAL_DBUS, test "x$have_external_dbus" = "xyes")

PKG_CHECK_MODULES(NOTIFY_SHARP, notify-sharp, have_external_notify_sharp="yes", have_external_notify_sharp="no")
if test "x$have_external_notify_sharp" != "xyes"; then
	AC_MSG_RESULT([notify-sharp not found - using internal copy])
fi
AM_CONDITIONAL(EXTERNAL_NOTIFY_SHARP, test "x$have_external_notify_sharp" = "xyes")


PKG_CHECK_MODULES(GNOMEKEYRINGSHARP, gnome-keyring-sharp-1.0 >= 1.0.0, \
	have_external_gks="yes", have_external_gks="no")
if test "x$have_external_gks" != "xyes"; then
	AC_MSG_RESULT([gnome-keyring-sharp not found - using internal copy])
fi
AM_CONDITIONAL(EXTERNAL_GKS, test "x$have_external_gks" = "xyes")

AC_PATH_PROG(MCS, gmcs)
if test x$MCS = x; then
	AC_MSG_ERROR(You need mcs)
fi

AC_PATH_PROG(MONO, mono)
if test x$MONO = x; then
	AC_MSG_ERROR(You need mono)
fi

MCS_FLAGS="-codepage:utf8"
MONO_FLAGS=
if test $USE_MAINTAINER_MODE = yes; then
	MCS_FLAGS="$MCS_FLAGS -debug"
	MONO_FLAGS="$MONO_FLAGS --debug"
fi
AC_SUBST(MCS_FLAGS)
AC_SUBST(MONO_FLAGS)

AC_PATH_PROG(GACUTIL, gacutil)
if test x$GACUTIL = x; then
	AC_MSG_ERROR(You need gacutil)
fi

GACUTIL_FLAGS='/package lastexit /gacdir $(libdir) /root $(DESTDIR)$(libdir)'
AC_SUBST(GACUTIL_FLAGS)

GETTEXT_PACKAGE="last-exit"
AC_SUBST(GETTEXT_PACKAGE)

ALL_LINGUAS="ar ca cs da de dz en_GB es fi fr gl it nb nl oc pl pt pt_BR ro sl sv tr zh_CN"
AM_GLIB_GNU_GETTEXT

AC_OUTPUT([
Makefile
xmlrpccs/Makefile
gnome-keyring-sharp/Makefile
notify-sharp/Makefile
data/Makefile
data/glade/Makefile
data/icons/Makefile
data/ui/Makefile
liblast-exit/Makefile
src/Defines.cs
src/Makefile
po/Makefile.in
])
