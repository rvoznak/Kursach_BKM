CREATE TABLE "public.teachers" (
	"id_t" serial NOT NULL,
	"name_t" TEXT NOT NULL,
	"second_name" TEXT NOT NULL,
	"subject" TEXT NOT NULL,
	"description_t" TEXT NOT NULL,
	"fix_time" TIME NOT NULL,
	"price" integer NOT NULL,
	"picture_t" bytea NOT NULL,
	"tg_name_t" TEXT NOT NULL,
	CONSTRAINT "teachers_pk" PRIMARY KEY ("id_t")
) WITH (
  OIDS=FALSE
);



CREATE TABLE "public.students" (
	"id_s" serial NOT NULL,
	"name_s" TEXT NOT NULL,
	"class" integer NOT NULL,
	"description_s" TEXT NOT NULL,
	"telephone_number_s" TEXT NOT NULL,
	"tg_name_s" TEXT NOT NULL,
	CONSTRAINT "students_pk" PRIMARY KEY ("id_s")
) WITH (
  OIDS=FALSE
);



CREATE TABLE "public.rating_teacher" (
	"id_rt" serial NOT NULL,
	"id_t" integer NOT NULL,
	"rating" FLOAT NOT NULL,
	CONSTRAINT "rating_teacher_pk" PRIMARY KEY ("id_rt")
) WITH (
  OIDS=FALSE
);



CREATE TABLE "public.passwd_t" (
	"id_pswd" serial NOT NULL,
	"id_t" integer NOT NULL,
	"passwd" TEXT NOT NULL,
	CONSTRAINT "passwd_t_pk" PRIMARY KEY ("id_pswd")
) WITH (
  OIDS=FALSE
);



CREATE TABLE "public.passwd_s" (
	"id_pswd" serial NOT NULL,
	"id_s" integer NOT NULL,
	"passwd" TEXT NOT NULL,
	CONSTRAINT "passwd_s_pk" PRIMARY KEY ("id_pswd")
) WITH (
  OIDS=FALSE
);



create table "students_teachers" (
	"id_st" serial NOT NULL ,
	"id_t" integer  REFERENCES "teachers" (id_t),
	"id_s" integer REFERENCES "students" (id_s),
	PRIMARY KEY (id_st),
	UNIQUE (id_t,id_s)
) WITH (
   OIDS=FALSE
);



ALTER TABLE "rating_teacher" ADD CONSTRAINT "rating_teacher_fk0" FOREIGN KEY ("id_t") REFERENCES "teachers"("id_t");

ALTER TABLE "passwd_t" ADD CONSTRAINT "passwd_t_fk0" FOREIGN KEY ("id_t") REFERENCES "teachers"("id_t");

ALTER TABLE "passwd_s" ADD CONSTRAINT "passwd_s_fk0" FOREIGN KEY ("id_s") REFERENCES "students"("id_s");







